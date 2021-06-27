using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Model.Settings
{
    public class ApplicationSettings : SettingsBase
    {
        private KeyGesture quickSearchGesture;
        private KeyGesture quickCreateGesture;
        private KeyGesture quickCreateWithOptionsGesture;
        private KeyGesture toggleMainWindowGesture;
        private DefaultWorkItemInfo defaultWorkItemInfo;
        private bool enableOfficeAddIns;
        private bool launchAnnotationToolAfterScreenCapture;
        private bool recordMicrophone;
        private bool searchAllInOutlook;
        private bool isTracingEnabled;
        private bool launchOnStartup;
        private bool showSplashScreen;
        private bool playNotificationSound;
        private TimeSpan refreshInterval;

        private ObservableCollection<ProjectInfo> projects = new ObservableCollection<ProjectInfo>();

        public event EventHandler IsTracingEnabledChanged;
        public event EventHandler SendAnonymousUsageDataChanged;
        public event EventHandler EnableOfficeAddInsChanged;
        public event EventHandler LaunchOnStartupChanged;
        public event EventHandler<ProjectsRemovedEventArgs> ProjectsRemoved;

        public static readonly TimeSpan MinimumRefreshInterval = TimeSpan.FromMinutes(15);
        public static readonly TimeSpan DefaultRefreshInterval = TimeSpan.FromMinutes(30);

        public ApplicationSettings()
        {
            this.projects.CollectionChanged += HandleProjectsChanged;

            // Default values
            this.EnableOfficeAddIns = true;
            this.RecordMicrophone = true;
            this.ShowCountdown = true;
            this.LaunchAnnotationToolAfterScreenCapture = true;
            this.LaunchOnStartup = true;
            this.ShowSplashScreen = true;
            this.PlayNotificationSound = true;

            this.QuickCreateGesture = TeamMateGestures.QuickCreate;
            this.QuickCreateWithOptionsGesture = TeamMateGestures.QuickCreateWithOptions;
            this.QuickSearchGesture = TeamMateGestures.QuickSearch;
            this.ToggleMainWindowGesture = TeamMateGestures.ToggleMainWindow;

            this.ShowItemCountInTaskBar = true;
            this.ShowItemCountInOverviewWindow = true;
            this.ShowItemCountInNotificationArea = true;

            this.RefreshInterval = ApplicationSettings.DefaultRefreshInterval;

            this.SearchIdsAutomatically = true;
            this.SendAnonymousUsageData = true;
        }

        public bool EnableOfficeAddIns
        {
            get { return this.enableOfficeAddIns; }
            set
            {
                if (SetProperty(ref this.enableOfficeAddIns, value))
                {
                    EnableOfficeAddInsChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool LaunchOnStartup
        {
            get { return this.launchOnStartup; }
            set
            {
                if (SetProperty(ref this.launchOnStartup, value))
                {
                    LaunchOnStartupChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ICollection<ProjectInfo> Projects
        {
            get { return this.projects; }
        }

        public KeyGesture QuickSearchGesture
        {
            get { return this.quickSearchGesture; }
            set { SetProperty(ref this.quickSearchGesture, value); }
        }

        public KeyGesture QuickCreateGesture
        {
            get { return this.quickCreateGesture; }
            set { SetProperty(ref this.quickCreateGesture, value); }
        }

        public KeyGesture QuickCreateWithOptionsGesture
        {
            get { return this.quickCreateWithOptionsGesture; }
            set { SetProperty(ref this.quickCreateWithOptionsGesture, value); }
        }

        public KeyGesture ToggleMainWindowGesture
        {
            get { return this.toggleMainWindowGesture; }
            set { SetProperty(ref this.toggleMainWindowGesture, value); }
        }

        public DefaultWorkItemInfo DefaultWorkItemInfo
        {
            get { return this.defaultWorkItemInfo; }
            set { SetProperty(ref this.defaultWorkItemInfo, value); }
        }

        public bool LaunchAnnotationToolAfterScreenCapture
        {
            get { return this.launchAnnotationToolAfterScreenCapture; }
            set { SetProperty(ref this.launchAnnotationToolAfterScreenCapture, value); }
        }

        public bool RecordMicrophone
        {
            get { return this.recordMicrophone; }
            set { SetProperty(ref this.recordMicrophone, value); }
        }

        public bool SearchAllInOutlook
        {
            get { return this.searchAllInOutlook; }
            set { SetProperty(ref this.searchAllInOutlook, value); }
        }

        public bool IsTracingEnabled
        {
            get { return this.isTracingEnabled; }
            set
            {
                if (SetProperty(ref this.isTracingEnabled, value))
                {
                    IsTracingEnabledChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public bool ShowSplashScreen
        {
            get { return this.showSplashScreen; }
            set { SetProperty(ref this.showSplashScreen, value); }
        }

        public bool PlayNotificationSound
        {
            get { return this.playNotificationSound; }
            set { SetProperty(ref this.playNotificationSound, value); }
        }

        public TimeSpan RefreshInterval
        {
            get { return this.refreshInterval; }
            set { SetProperty(ref this.refreshInterval, value); }
        }

        private bool searchIdsAutomatically;

        public bool SearchIdsAutomatically
        {
            get { return this.searchIdsAutomatically; }
            set { SetProperty(ref this.searchIdsAutomatically, value); }
        }

        private bool sendAnonymousUsageData;

        public bool SendAnonymousUsageData
        {
            get { return this.sendAnonymousUsageData; }
            set
            {
                if (SetProperty(ref this.sendAnonymousUsageData, value))
                {
                    SendAnonymousUsageDataChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private bool showCountdown;

        public bool ShowCountdown
        {
            get { return this.showCountdown; }
            set { SetProperty(ref this.showCountdown, value); }
        }


        private bool showItemCountInOverviewWindow;

        public bool ShowItemCountInOverviewWindow
        {
            get { return this.showItemCountInOverviewWindow; }
            set { SetProperty(ref this.showItemCountInOverviewWindow, value); }
        }

        private bool showItemCountInNotificationArea;

        public bool ShowItemCountInNotificationArea
        {
            get { return this.showItemCountInNotificationArea; }
            set { SetProperty(ref this.showItemCountInNotificationArea, value); }
        }

        private bool showItemCountInTaskBar;

        public bool ShowItemCountInTaskBar
        {
            get { return this.showItemCountInTaskBar; }
            set { SetProperty(ref this.showItemCountInTaskBar, value); }
        }

        private void HandleProjectsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged("Projects");

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (ProjectsRemoved != null)
                {
                    var removedProjects = e.OldItems.OfType<ProjectInfo>().ToArray();
                    ProjectsRemoved(this, new ProjectsRemovedEventArgs(removedProjects));
                }
            }
        }
    }

    public class ProjectsRemovedEventArgs : EventArgs
    {
        public ProjectsRemovedEventArgs(ProjectInfo[] projects)
        {
            Assert.ParamIsNotNull(projects, "projects");
            this.RemovedProjects = projects;
        }

        public ProjectInfo[] RemovedProjects { get; private set; }
    }
}