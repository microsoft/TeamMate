// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.ViewModels
{
    public class SettingsPageViewModel : PageViewModelBase
    {
        private static readonly IList<Tuple<TimeSpan, string>> RefreshIntervals = new Tuple<TimeSpan, string>[] {
            new Tuple<TimeSpan,string>(TimeSpan.FromMinutes(15), "15 Minutes"),
            new Tuple<TimeSpan,string>(TimeSpan.FromMinutes(30), "30 Minutes"),
            new Tuple<TimeSpan,string>(TimeSpan.FromHours(1), "1 Hour"),
            new Tuple<TimeSpan,string>(TimeSpan.FromHours(2), "2 Hours"),
            new Tuple<TimeSpan,string>(TimeSpan.FromHours(4), "4 Hours"),
            new Tuple<TimeSpan,string>(TimeSpan.FromMinutes(8), "8 Hours"),
            new Tuple<TimeSpan,string>(TimeSpan.FromDays(1), "1 Day"),
            new Tuple<TimeSpan,string>(TimeSpan.MaxValue, "Never"),
        };

        private ApplicationSettings settings;
        private ProjectSettings projectSettings;

        public SettingsPageViewModel()
        {
            this.Title = "Settings";
            this.OpenLogsFolderCommand = new RelayCommand(OpenLogsFolder);
        }

        public void OpenLogsFolder()
        {
            string logsFolder = this.LogsFolder;
            if (Directory.Exists(logsFolder))
            {
                Process.Start(logsFolder);
            }
            else
            {
                this.MessageBoxService.ShowError(this, String.Format("The log folder {0} doesn't exist yet, so it couldn't be opened.", logsFolder));
            }
        }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public ExternalWebBrowserService ExternalWebBrowserService { get; set; }

        public ICommand OpenLogsFolderCommand { get; private set; }

        public string LogsFolder
        {
            get
            {
                return TeamMateApplicationInfo.LogsFolder;
            }
        }

        public ApplicationSettings Settings
        {
            get { return this.settings; }
            set
            {
                if (SetProperty(ref this.settings, value))
                {
                    LoadSettings();
                }
            }
        }

        public ProjectSettings ProjectSettings
        {
            get { return this.projectSettings; }
            set
            {
                if (SetProperty(ref this.projectSettings, value))
                {
                    LoadProjectSettings();
                }
            }
        }

        /// <summary>
        /// Index of the selected refresh interval from the <c>RefreshIntervals</c> list.
        /// -1 set as default value as 0 is a legal value, and SetProperty needs to fire in this case too.
        /// </summary>
        private int refreshIntervalTick = -1;

        public int RefreshIntervalTick
        {
            get { return this.refreshIntervalTick; }
            set
            {
                if (SetProperty(ref this.refreshIntervalTick, value))
                {
                    var interval = GetSelectedRefreshInterval();
                    this.RefreshIntervalDescription = (interval != null) ? interval.Item2 : null;

                    InvalidateSettings();
                }
            }
        }

        private string refreshIntervalDescription;

        public string RefreshIntervalDescription
        {
            get { return this.refreshIntervalDescription; }
            set { SetProperty(ref this.refreshIntervalDescription, value); }
        }


        private int CalculateRefreshIntervalTick(TimeSpan refreshInterval)
        {
            int refreshIntervalTick = RefreshIntervals.Count - 1;

            for (int i = 0; i < RefreshIntervals.Count; i++)
            {
                if (RefreshIntervals[i].Item1 == refreshInterval)
                {
                    refreshIntervalTick = i;
                    break;
                }
            }

            return refreshIntervalTick;
        }

        private Tuple<TimeSpan, string> GetSelectedRefreshInterval()
        {
            bool withinBounds = (refreshIntervalTick >= 0 && refreshIntervalTick < RefreshIntervals.Count);
            return (withinBounds) ? RefreshIntervals[refreshIntervalTick] : null;
        }

        private bool isLoadingSettings;

        private void LoadSettings()
        {
            if (this.settings != null)
            {
                isLoadingSettings = true;
                this.RefreshIntervalTick = CalculateRefreshIntervalTick(settings.RefreshInterval);
                isLoadingSettings = false;
            }
        }

        private void InvalidateSettings()
        {
            if (!isLoadingSettings)
            {
                if (this.Settings != null)
                {
                    var interval = GetSelectedRefreshInterval();
                    settings.RefreshInterval = (interval != null) ? interval.Item1 : TimeSpan.MaxValue;
                }
            }
        }

        private bool isLoadingProjectSettings;

        private void LoadProjectSettings()
        {
            if (this.projectSettings != null)
            {
                isLoadingProjectSettings = true;
                isLoadingProjectSettings = false;
            }
        }

        private void InvalidateProjectSettings()
        {
            if (!isLoadingProjectSettings && this.projectSettings != null)
            {
            }
        }
    }
}
