using Microsoft.Tools.TeamMate.Foundation.Chaos;
using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.Shell;
using Microsoft.Tools.TeamMate.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using SplashScreen = Microsoft.Tools.TeamMate.Windows.SplashScreen;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ApplicationService
    {
        private static readonly TimeSpan MinimumSplashScreenDisplayTime = TimeSpan.FromSeconds(1);

        private ApplicationInstance applicationInstace;
        private DateTime startTime;

        [Import]
        public WindowService WindowService { get; set; }

        [Import]
        public TelemetryService TelemetryService { get; set; }

        [Import]
        public UpgradeService UpgradeService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public UIService UIService { get; set; }

        [Import]
        public BackgroundTaskService BackgroundTaskService { get; set; }

        [Import]
        public TracingService TracingService { get; set; }

        [Import]
        public ToastNotificationService ToastNotificationService { get; set; }

        [Import]
        public CommandLineService CommandLineService { get; set; }

        [Import]
        public ConfigurationService ConfigurationService { get; set; }

        [Import]
        public ProjectDataService ProjectDataService { get; set; }

        [Import]
        public ViewService ViewService { get; set; }

        [Import]
        public VstsConnectionService VstsConnectionService { get; set; }

        [Import]
        public TrackingService TrackingService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public HistoryService HistoryService { get; set; }

        public async Task StartAsync()
        {
            try
            {
                // Initialize tracing service right away to enable logging as the first thing...
                EnsureTracingEnabled();

                // First, check if there is already a running instance. If so, forward arguments to it and exit.
                string[] launchArgs = GetLaunchArgs();
                this.applicationInstace = EnsureSingleInstanceOrExit(launchArgs);

                // If we got to that point, this is where the serious initialization should begin, another instance was not reactivated
                ChaosMonkey.IsEnabled = TeamMateApplicationInfo.IsDebugBuild;

                // Initialize telemetry service too 
                await this.TelemetryService.InitializeAsync();

                // Important to do this as first thing
                TeamMateApplicationInfo.Initialize();

                // In a weird case scenario, you are running an older version of the application, if so, downgrade
                this.UpgradeService.DowngradeIfNeeded();

                // If no running instance was found, perform upgrade before any settings or files are read from the data directory
                this.UpgradeService.UpgradeIfNeeded();

                // Flush current version and launch date to history
                TeamMateApplicationInfo.LastVersion = TeamMateApplicationInfo.Version;
                this.startTime = DateTime.Now;

                await InitializeAsync();

                if (TeamMateApplicationInfo.IsFirstRun)
                {
                    this.WindowService.ShowWelcomeDialog();

                    PostInitialize();
                }

                if (launchArgs.Length == 0)
                {
                    this.WindowService.ShowMainWindow();
                }
                else
                {
                    ProcessCommandLineArgs(launchArgs);
                }
            }
            catch (Exception e)
            {
                // Make sure logging is enabled and trace this unexpected error
                EnsureTracingEnabled();
                Log.ErrorAndBreak(e);
                throw;
            }
        }

        public void Shutdown()
        {
            try
            {
                // Update Last Run Time
                DateTime now = DateTime.Now;

                var history = this.HistoryService.History;
                history.LastRun = now;

                if (startTime != default(DateTime))
                {
                    history.Uptime += (now - startTime);
                }

                if (this.applicationInstace != null)
                {
                    this.applicationInstace.Dispose();
                    this.applicationInstace = null;
                }
            }
            catch (Exception e)
            {
                // Make sure logging is enabled and trace this unexpected error
                EnsureTracingEnabled();
                Log.ErrorAndBreak(e);
                throw;
            }
        }

        private static string[] GetLaunchArgs()
        {
            string[] launchArgs = TeamMateApplicationInfo.CommandLineArgs;
            return launchArgs;
        }

        private ApplicationInstance EnsureSingleInstanceOrExit(string[] args)
        {
            ApplicationInstance instance = ApplicationInstance.GetOrCreate(TeamMateApplicationInfo.ApplicationName);
            if (!instance.Owned)
            {
                // Delegate to existing running instance, and exit gracefully
                instance.SendMessage(args);
                Environment.Exit(0);
            }

            instance.MessageReceived += HandleApplicationMessageReceived;
            return instance;
        }

        private async Task InitializeAsync()
        {
            List<Task> tasks = new List<Task>();
            SplashScreen splashScreen = null;


            if (this.SettingsService.Settings.ShowSplashScreen)
            {
                splashScreen = new SplashScreen();
                splashScreen.Show();

                // Add an aditional task to make sure we display the splash screen for a minimum amount of time.
                tasks.Add(Task.Delay(MinimumSplashScreenDisplayTime));
            }

            try
            {
                tasks.Add(Task.Run(() => InitializeInBackground()));
                await Task.WhenAll(tasks.ToArray());

                InitializeInForeground();
            }
            finally
            {
                if (splashScreen != null)
                {
                    splashScreen.Close();
                }
            }
        }

        private void EnsureTracingEnabled()
        {
            // Tracing is turned on by default until we get to read the user settings, then we can tweak it...
            this.TracingService.IsTracingEnabled = true;
        }

        private void InitializeInBackground()
        {
            // Tracing
            InvalidateIsTracingEnabled();
            this.SettingsService.Settings.IsTracingEnabledChanged += HandleIsTracingEnabledChanged;

            // Telemetry
            InvalidateIsTelemetryEnabled();
            this.SettingsService.Settings.SendAnonymousUsageDataChanged += HandleSendAnonymousUsageDataChanged;

            // Services
            this.ConfigurationService.Initialize();
            this.ProjectDataService.Initialize();
            this.ViewService.Initialize();
            this.TrackingService.Initialize();
            this.BackgroundTaskService.Initialize();
            this.TracingService.Initialize();

            if (this.UpgradeService.ShouldDisplayLegacyTfsSupportDroppedBanner)
            {
                this.SettingsService.VolatileSettings.DisplayLegacyTfsSupportDroppedBanner = true;
            }

            // Update Last Run Time (after initializing services)
            var history = this.HistoryService.History;
            DateTime now = DateTime.Now;
            history.LastRun = now;

            if (history.FirstRun == null)
            {
                history.FirstRun = now;
            }

            history.Launches++;
        }

        private void InitializeInForeground()
        {
            this.UIService.Initialize();
            this.ToastNotificationService.Initialize();

            if (!TeamMateApplicationInfo.IsFirstRun)
            {
                // If first run, we need to do this post splash screen, after we show a welcome window
                PostInitialize();
            }
        }

        private void PostInitialize()
        {
            this.ConfigurationService.Configure();
            this.UIService.RegisterHotKeys();

            // Starting in foreground as it registers a session notification helper, which needs to be in the right thread
            this.BackgroundTaskService.Start();

            // Needs to happen after configure ran...
            AutoconnectToLastProject();
        }

        private void InvalidateIsTracingEnabled()
        {
            this.TracingService.IsTracingEnabled = this.SettingsService.Settings.IsTracingEnabled;
        }

        private void HandleIsTracingEnabledChanged(object sender, EventArgs e)
        {
            InvalidateIsTracingEnabled();
        }

        private void InvalidateIsTelemetryEnabled()
        {
            this.TelemetryService.IsTelemetryEnabled = this.SettingsService.Settings.SendAnonymousUsageData;
        }

        private void HandleSendAnonymousUsageDataChanged(object sender, EventArgs e)
        {
            InvalidateIsTelemetryEnabled();
        }

        private void AutoconnectToLastProject()
        {
            var defaultProjectInfo = this.SettingsService.VolatileSettings.LastUsedProject;
            if (defaultProjectInfo != null)
            {
                this.VstsConnectionService.BeginConnect(defaultProjectInfo);
            }
        }

        private void ProcessCommandLineArgs(string[] args)
        {
            this.CommandLineService.Execute(args);
        }

        private void HandleApplicationMessageReceived(ApplicationInstance instance, object message)
        {
            string[] args = message as string[];
            if (args != null && !args.Any(arg => arg == null))
            {
                // TODO: Queue these in case the startup portion is not done yet (e.g. downloading assemblies?).
                // At the end of startup, we should dequeue any command line requests?
                ProcessCommandLineArgs(args);
            }
        }
    }
}
