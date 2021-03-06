﻿namespace Shapeshifter.WindowsDesktop.Controls.Clipboard.Designer.Services
{
    using System;

    using WindowsDesktop.Services.Processes.Interfaces;

    using Controls.Designer.Services;

    class DesignerProcessManager
        : IProcessManager,
          IDesignerService
    {
		public string CurrentProcessName => throw new NotImplementedException();

		public int CurrentProcessId => throw new NotImplementedException();

		public void LaunchCommand(string command, string arguments = null) { }

        public void CloseAllDuplicateProcessesExceptCurrent() { }

        public void LaunchFile(string fileName, string arguments = null) { }

        public void LaunchFileWithAdministrativeRights(string fileName, string arguments = null) { }

        public bool IsCurrentProcessElevated()
        {
            return false;
        }

        public void Dispose() { }

        public string GetCurrentProcessDirectory()
        {
            return null;
        }

		public string GetCurrentProcessFilePath()
		{
			throw new NotImplementedException();
		}

		public void CloseCurrentProcess()
		{
			throw new NotImplementedException();
		}
	}
}