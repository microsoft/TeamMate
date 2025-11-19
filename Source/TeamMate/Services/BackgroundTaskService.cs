using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class BackgroundTaskService : IDisposable
    {
        private List<ScheduledAction> scheduledActions = new List<ScheduledAction>();
        private ScheduledAction refreshTilesAction;

        private SessionNotificationHelper SessionNotificationHelper { get; set; }

        private bool started;

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        public void Initialize()
        {
            this.scheduledActions.AddRange(new ScheduledAction[] {
                this.WindowService.RefreshTilesAction
            });

            this.refreshTilesAction = this.WindowService.RefreshTilesAction;

            this.SettingsService.Settings.PropertyChanged += HandleSettingsChanged;

            var interval = this.SettingsService.Settings.RefreshInterval;
            if (IsValid(interval))
            {
                refreshTilesAction.Interval = interval;
            }
        }

        public void Start()
        {
            if (!started)
            {
                started = true;

                SessionNotificationHelper = new SessionNotificationHelper();
                SessionNotificationHelper.SessionLocked += HandleSessionLocked;
                SessionNotificationHelper.SessionUnlocked += HandleSessionUnlocked;

                StartActions();
            }
        }

        public void Stop()
        {
            if (started)
            {
                started = false;

                if (SessionNotificationHelper != null)
                {
                    SessionNotificationHelper.SessionLocked -= HandleSessionLocked;
                    SessionNotificationHelper.SessionUnlocked -= HandleSessionUnlocked;
                    SessionNotificationHelper.Dispose();
                    SessionNotificationHelper = null;
                }

                StopActions();
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void StartActions()
        {
            foreach (ScheduledAction action in this.scheduledActions)
            {
                if (IsValid(action.Interval))
                {
                    action.Start();
                }
            }
        }

        private void StopActions()
        {
            foreach (ScheduledAction action in this.scheduledActions)
            {
                action.Stop();
            }
        }

        private void InvalidateRefreshInterval()
        {
            var interval = this.SettingsService.Settings.RefreshInterval;
            if (IsValid(interval))
            {
                refreshTilesAction.Interval = interval;
                if (!refreshTilesAction.IsRunning)
                {
                    refreshTilesAction.Start();
                }
            }
            else
            {
                refreshTilesAction.Stop();
            }
        }

        private static bool IsValid(TimeSpan interval)
        {
            return interval < TimeSpan.MaxValue && interval > TimeSpan.Zero;
        }

        private void HandleSettingsChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RefreshInterval")
            {
                InvalidateRefreshInterval();
            }
        }

        private void HandleSessionUnlocked(object sender, EventArgs e)
        {
            StartActions();
        }

        private void HandleSessionLocked(object sender, EventArgs e)
        {
            StopActions();
        }
    }
}
