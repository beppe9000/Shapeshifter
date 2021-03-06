﻿namespace Shapeshifter.WindowsDesktop.Services.Web.Updates
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Text.RegularExpressions;
	using System.Threading.Tasks;

	using Files.Interfaces;

	using Interfaces;

	using Octokit;

	using Processes.Interfaces;

	using Web.Interfaces;
	using Infrastructure.Environment.Interfaces;
	using Serilog;
	using Shapeshifter.WindowsDesktop.Services.Interfaces;

	class UpdateService
        : IUpdateService
    {
        const string RepositoryOwner = "ffMathy";
        const string RepositoryName = "Shapeshifter";

        readonly IGitHubClient client;
		readonly IDownloader fileDownloader;
        readonly IFileManager fileManager;
        readonly IProcessManager processManager;
        readonly ILogger logger;
        readonly IEnvironmentInformation environmentInformation;
		readonly ISettingsManager settingsManager;

		public UpdateService(
            IDownloader fileDownloader,
            IFileManager fileManager,
            IProcessManager processManager,
            ILogger logger,
            IGitHubClientFactory clientFactory,
            IEnvironmentInformation environmentInformation,
			ISettingsManager settingsManager)
        {
            client = clientFactory.CreateClient();
			
			this.fileDownloader = fileDownloader;
            this.fileManager = fileManager;
            this.processManager = processManager;
            this.logger = logger;
            this.environmentInformation = environmentInformation;
			this.settingsManager = settingsManager;
		}

        public async Task UpdateAsync()
        {
            if (!environmentInformation.GetHasInternetAccess()) return;
            if (!environmentInformation.GetShouldUpdate()) return;

            try
            {
                var pendingUpdateRelease = await GetAvailableUpdateAsync();
                if (pendingUpdateRelease == null)
                {
                    return;
                }

				await UpdateFromReleaseAsync(pendingUpdateRelease);
            }
            catch (RateLimitExceededException)
            {
                logger.Warning(
                    "Did not search for updates due to the GitHub rate limit being exceed.");
            }
            catch (HttpRequestException)
            {
                logger.Warning(
                    "Could not search for updates due to a connection issue.");
            }
        }

        async Task UpdateFromReleaseAsync(Release pendingUpdateRelease)
        {
            var assets =
                await client
                    .Repository
                    .Release
                    .GetAllAssets(
                        RepositoryOwner,
                        RepositoryName,
                        pendingUpdateRelease.Id);
            await UpdateFromAssetsAsync(assets);
        }

        async Task UpdateFromAssetsAsync(IEnumerable<ReleaseAsset> assets)
        {
            const string targetAssetName = "Shapeshifter.exe";

            var asset = assets.Single(x => x.Name == targetAssetName);
            await UpdateFromAssetAsync(asset);
        }

        async Task UpdateFromAssetAsync(ReleaseAsset asset)
        {
            var localFilePath = await DownloadUpdateToDiskAsync(asset);
            processManager.LaunchFile(
                localFilePath,
                "update");
        }

        async Task<string> DownloadUpdateToDiskAsync(ReleaseAsset asset)
        {
            var data = await fileDownloader.DownloadBytesAsync(
                asset.BrowserDownloadUrl);
            var localFilePath = await fileManager.WriteBytesToTemporaryFileAsync(
                asset.Name,
                data);

            return localFilePath;
        }

        bool IsUpdateToReleaseNeeded(Release release)
        {
			var shouldUpdateToPrerelease = settingsManager.LoadSetting("PreferPrerelease", false);
            if (release.Prerelease && !shouldUpdateToPrerelease)
                return false;

            if (release.Draft)
            {
                return false;
            }

            var releaseVersion = GetReleaseVersion(release);
            return IsUpdateToVersionNeeded(releaseVersion);
        }

        bool IsUpdateToVersionNeeded(Version releaseVersion)
        {
            return releaseVersion > Program.GetCurrentVersion();
        }

        static Version GetReleaseVersion(Release release)
        {
            var versionMatch = Regex.Match(release.Name, @"shapeshifter-v([\.\d]+)");
            var versionGroup = versionMatch.Groups[1];

            var version = new Version(versionGroup.Value);
            return version;
        }

        async Task<Release> GetAvailableUpdateAsync()
        {
            var updates = await GetReleasesWithUpdatesAsync();
            return updates
                .OrderByDescending(GetReleaseVersion)
                .FirstOrDefault();
        }

		bool IsCurrentRelease(Release release) {
			var releaseVersion = GetReleaseVersion(release);
			return releaseVersion == Program.GetCurrentVersion();
		}

        async Task<IEnumerable<Release>> GetReleasesWithUpdatesAsync()
        {
            var allReleases = await client.Repository.Release.GetAll(
                RepositoryOwner,
                RepositoryName);

			var currentRelease = allReleases.SingleOrDefault(IsCurrentRelease);
			if(currentRelease == null) {
				logger.Warning("Could not find the current release version v{version}.", Program.GetCurrentVersion());
			} else if(currentRelease.Prerelease) {
				settingsManager.SaveSetting("PreferPrerelease", true);
			}

            return allReleases.Where(IsUpdateToReleaseNeeded);
        }
    }
}