using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.Model;
using Microsoft.Internal.Tools.TeamMate.Model.Settings;
using Microsoft.Internal.Tools.TeamMate.Resources;
using Microsoft.Internal.Tools.TeamMate.Services;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, ICommandProvider
    {
        private bool isSettingsVisible;
        private Session session;

        public MainWindowViewModel()
        {
            this.HomePage = ViewModelFactory.Create<HomePageViewModel>();
            this.NewWorkItemPage = ViewModelFactory.Create<NewWorkItemPageViewModel>();
            this.ProjectsPage = ViewModelFactory.Create<ProjectsPageViewModel>();
            this.DeveloperOptionsPage = ViewModelFactory.Create<DeveloperOptionsPageViewModel>();
            this.SettingsPage = ViewModelFactory.Create<SettingsPageViewModel>();

            this.Navigation = ViewModelFactory.Create<NavigationViewModel>();
            this.Navigation.PropertyChanged += HandleNavigationPropertyChanged;
            this.NavigateToHomePage();

            InvalidateCurrentPage();
        }

        public void Hide()
        {
            this.UIService.ShowTrayIconReminder();
        }

        [Import]
        public ExternalWebBrowserService ExternalWebBrowserService { get; set; }

        [Import]
        public UIService UIService { get; set; }

        [Import]
        public VstsConnectionService VstsConnectionService { get; set; }


        [Import]
        public GlobalCommandService GlobalCommandService { get; set; }

        [Import]
        public WindowService WindowService { get; set; }

        public void RegisterBindings(CommandBindingCollection bindings)
        {
            this.Navigation.RegisterBindings(bindings);

            bindings.Add(TeamMateCommands.SendASmile, () => this.WindowService.ShowSendASmileDialog(this));
            bindings.Add(TeamMateCommands.SendAFrown, () => this.WindowService.ShowSendAFrownDialog(this));
            this.GlobalCommandService.RegisterBindings(bindings);

            bindings.Add(TeamMateCommands.NavigateToHomePage, this.NavigateToHomePage);
            bindings.Add(TeamMateCommands.NavigateToNewWorkItemPage, this.NavigateToNewWorkItemPage);
            bindings.Add(TeamMateCommands.NavigateToProjectsPage, () => this.NavigateToProjectsPage());
            bindings.Add(TeamMateCommands.NavigateToDeveloperOptionsPage, () => this.NavigateTo(this.DeveloperOptionsPage));
            bindings.Add(TeamMateCommands.NavigateToSettingsPage, NavigateToSettingsPage);
            bindings.Add(TeamMateCommands.NavigateToNewsPage, this.LaunchNewsPage);

            bindings.Add(TeamMateCommands.ConnectToProject, ConnectToProject);
            bindings.Add(TeamMateCommands.RetryConnectToTfs, RetryConnectToTfs);
        }

        public NavigationViewModel Navigation { get; set; }

        public HomePageViewModel HomePage { get; private set; }

        public NewWorkItemPageViewModel NewWorkItemPage { get; private set; }

        public ProjectsPageViewModel ProjectsPage { get; private set; }

        public SettingsPageViewModel SettingsPage { get; private set; }

        public DeveloperOptionsPageViewModel DeveloperOptionsPage { get; private set; }

        public bool IsSettingsVisible
        {
            get { return this.isSettingsVisible; }
            private set { SetProperty(ref this.isSettingsVisible, value); }
        }

        public bool IsDeveloperOptionsButtonVisible
        {
            get { return TeamMateApplicationInfo.IsDebugBuild; }
        }

        private bool isUpdateAvailable;

        public bool IsUpdateAvailable
        {
            get { return this.isUpdateAvailable; }
            set { SetProperty(ref this.isUpdateAvailable, value); }
        }

        public object CurrentPage
        {
            get { return this.Navigation.Page; }
        }

        public Session Session
        {
            get { return this.session; }
            set
            {
                Session oldSession = this.session;
                if (SetProperty(ref this.session, value))
                {
                    if (oldSession != null)
                    {
                        oldSession.ProjectContextChanged -= HandleProjectContextChanged;
                    }

                    if (session != null)
                    {
                        session.ProjectContextChanged += HandleProjectContextChanged;
                    }

                    HomePage.Session = session;
                    NewWorkItemPage.Session = session;
                    InvalidateProjectContext();
                }
            }
        }

        public void Refresh()
        {
            HomePage.Refresh();
        }

        private void InvalidateProjectContext()
        {
            ProjectContext projectContext = (session != null) ? session.ProjectContext : null;

            this.HomePage.TileCollection.TileGroupName = (projectContext != null) ? projectContext.ProjectName : null;
            this.HomePage.TileCollection.RawTiles = (projectContext != null) ? projectContext.Tiles : null;
            this.NewWorkItemPage.WorkItemTypes = (projectContext != null) ? projectContext.WorkItemTypes : null;
            this.SettingsPage.ProjectSettings = (projectContext != null) ? projectContext.ProjectSettings : null;
        }

        private void HandleNavigationPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Page")
            {
                InvalidateCurrentPage();
            }
        }

        private void InvalidateCurrentPage()
        {
            this.IsSettingsVisible = (CurrentPage == SettingsPage);
            this.CurrentPageSupportsFiltering = (CurrentPage is IFilterable);
        }

        private bool currentPageSupportsFiltering;

        public bool CurrentPageSupportsFiltering
        {
            get { return this.currentPageSupportsFiltering; }
            set { SetProperty(ref this.currentPageSupportsFiltering, value); }
        }

        private void HandleProjectContextChanged(object sender, EventArgs e)
        {
            InvalidateProjectContext();

            // When we reconnect to a new project refresh the queries
            Refresh();
        }

        public void NavigateToHomePage()
        {
            this.NavigateTo(this.HomePage);
        }

        [Import]
        public SettingsService SettingsService { get; set; }

        public void NavigateToSettingsPage()
        {
            if (!IsSettingsVisible)
            {
                this.SettingsPage.Settings = this.SettingsService.Settings;
                this.NavigateTo(this.SettingsPage);
            }
        }

        public void NavigateToNewWorkItemPage()
        {
            this.NavigateTo(this.NewWorkItemPage);
        }

        private void NavigateToProjectsPage(bool launchSelectProjectDialog = false)
        {
            this.NavigateTo(this.ProjectsPage);

            if (launchSelectProjectDialog)
            {
                ProjectsPage.ChooseProjects();
            }
        }

        public VolatileSettings VolatileSettings
        {
            get { return this.SettingsService.VolatileSettings; }
        }

        private string searchText;

        public string SearchText
        {
            get { return this.searchText; }
            set { SetProperty(ref this.searchText, value); }
        }

        public void PerformSearch()
        {
            Search(this.SearchText, false);
        }

        [Import]
        public SessionService SessionService { get; set; }


        public void Search(string searchText, bool triggeredFromQuickSearch, bool setSearchText = false)
        {
            searchText = TextMatcher.NormalizeSearchText(searchText);

            TelemetryEventProperties properties = null;
            if (triggeredFromQuickSearch)
            {
                properties = new TelemetryEventProperties() {
                    { TelemetryEvents.Properties.QuickSearch, true }
                };
            }

            if (!String.IsNullOrWhiteSpace(searchText))
            {
                int workItemId;
                if (WorkItemReference.TryParseId(searchText, out workItemId))
                {
                    WorkItemReference reference = new WorkItemReference(this.SessionService.Session.ProjectContext.ProjectInfo.ProjectCollectionUri, workItemId);
                    Telemetry.Event(TelemetryEvents.WorkItemOpenedUsingSearch, properties);
                    this.WindowService.ShowWorkItemWindow(reference);
                }
                else
                {
                    // Make sure the main window is visible and in the foreground. 
                    // TODO: Ugly, view model should not know about the main window.
                    this.WindowService.ShowMainWindow(true);

                    if (setSearchText)
                    {
                        this.SearchText = searchText;

                        // TODO: Request select all and focus search text?
                    }

                    Telemetry.Event(TelemetryEvents.Search, properties);
                    NavigateToSearchPage(searchText);
                }
            }
        }


        private void NavigateToSearchPage(string searchText)
        {
            SearchPageViewModel pageViewModel = CurrentPage as SearchPageViewModel;
            if (pageViewModel != null)
            {
                // TODO: Stop having search text, instead set a search expression
                pageViewModel.SearchText = searchText;
            }
            else
            {
                pageViewModel = ViewModelFactory.Create<SearchPageViewModel>();
                pageViewModel.SearchText = searchText;
                NavigateTo(pageViewModel);
            }
        }

        private void NavigateTo(PageViewModelBase page)
        {
            Navigation.NavigateToPage(page);
        }

        public void ConnectToProject()
        {
            NavigateToProjectsPage(true);
        }

        public async void RetryConnectToTfs()
        {
            await this.VstsConnectionService.RetryConnectAsync();
        }

        public void QuickCreateDefault()
        {
            this.GlobalCommandService.QuickCreateDefault();
        }

        private void LaunchNewsPage()
        {
            this.ExternalWebBrowserService.LaunchNewsPage();
        }

        private ICommand displayLegacyTfsSupportDroppedBannerCommand;

        public ICommand DisplayLegacyTfsSupportDroppedBannerCommand
        {
            get
            {
                return displayLegacyTfsSupportDroppedBannerCommand ?? (displayLegacyTfsSupportDroppedBannerCommand = new RelayCommand(() =>
                {
                    this.ExternalWebBrowserService.LaunchLegacyTfsSupportDropped();
                }));
            }
        }


        private ICommand closeLegacyTfsSupportDroppedBannerCommand;

        public ICommand CloseLegacyTfsSupportDroppedBannerCommand
        {
            get
            {
                return closeLegacyTfsSupportDroppedBannerCommand ?? (closeLegacyTfsSupportDroppedBannerCommand = new RelayCommand(() =>
                {
                    this.VolatileSettings.DisplayLegacyTfsSupportDroppedBanner = false;
                }));
            }
        }
    }
}
