// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows;
using System;
using System.Windows;
using TeamMateApplication = Microsoft.Tools.TeamMate.Model.Application;

namespace Microsoft.Tools.TeamMate
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public TeamMateApplication Application { get; private set; }

        public static new App Current
        {
            get { return System.Windows.Application.Current as App; }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            try
            {
                TeamMateApplication application = new TeamMateApplication();
                await application.StartAsync();

                this.Application = application;
            }
            catch (Exception ex)
            {
                UserFeedback.ShowError(ex);
                Environment.Exit(1);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            try
            {
                if (this.Application != null)
                {
                    this.Application.Shutdown();
                    this.Application = null;
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                UserFeedback.ShowError(ex);
                Environment.Exit(1);
            }
        }
    }
}
