﻿#region

using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using Autofac;
using Shapeshifter.UserInterface.WindowsDesktop.Infrastructure.Dependencies;

#endregion

namespace Shapeshifter.UserInterface.WindowsDesktop
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    [ExcludeFromCodeCoverage]
    public partial class App : Application
    {
        private static ILifetimeScope container;

        private static ILifetimeScope Container
        {
            get
            {
                if (container == null)
                {
                    CreateContainer();
                }
                return container;
            }
        }

        public static void CreateContainer(Action<ContainerBuilder> callback = null)
        {
            lock (typeof (App))
            {
                var builder = new ContainerBuilder();
                builder.RegisterModule(new DefaultWiringModule());
                container = builder.Build();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            container.Dispose();
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, exceptionEventArguments) =>
            {
                MessageBox.Show(exceptionEventArguments.ExceptionObject.ToString(), "Shapeshifter error",
                    MessageBoxButton.OK);
                Current.Shutdown();
            };

            var main = Container.Resolve<Main>();
            main.Start(e.Args);
        }
    }
}