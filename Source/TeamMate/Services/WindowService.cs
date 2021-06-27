// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Foundation.Threading;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Model;
using Microsoft.Tools.TeamMate.Model.Settings;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.Tools.TeamMate.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows;
using WindowsForms = System.Windows.Forms;

namespace Microsoft.Tools.TeamMate.Services
{
    public class WindowService
    {
        private QuickSearchWindow quickSearchWindow;
        private MainWindow mainWindow;
        private MainWindowViewModel mainWindowViewModel;

        public ScheduledAction RefreshTilesAction { get; private set; }

        public WindowService()
        {
            WindowStateTracker.Instance.GetStoredStateDelegate = delegate (Window window)
            {
                return VolatileSettings.GetLastKnownState(window);
            };

            WindowStateTracker.Instance.StoreStateDelegate = delegate (Window window, WindowStateInfo state)
            {
                VolatileSettings.SetLastKnownState(window, state);
            };

            RefreshTilesAction = new ScheduledAction();
            RefreshTilesAction.Action = RefreshTiles;
        }

        [Import]
        public ViewService ViewService { get; set; }

        [Import]
        public ExternalWebBrowserService ExternalWebBrowserService { get; set; }

        [Import]
        public SettingsService SettingsService { get; set; }

        [Import]
        public MessageBoxService MessageBoxService { get; set; }

        [Import]
        public TrackingService TrackingService { get; set; }

        [Import]
        public SessionService SessionService { get; set; }

        [Import]
        public HistoryService HistoryService { get; set; }

        private WindowCollection Windows
        {
            get
            {
                return System.Windows.Application.Current.Windows;
            }
        }

        public MainWindowViewModel MainWindowViewModel
        {
            get
            {
                if (mainWindowViewModel == null)
                {
                    mainWindowViewModel = ViewModelFactory.Create<MainWindowViewModel>();
                    mainWindowViewModel.Session = this.SessionService.Session;
                }

                return mainWindowViewModel;
            }
        }

        public MainWindow MainWindow
        {
            get
            {
                if (mainWindow == null)
                {
                    mainWindow = new MainWindow();
                    mainWindow.ViewService = this.ViewService;
                    mainWindow.DataContext = MainWindowViewModel;

                    WindowStateTracker.Instance.Track(mainWindow);
                }

                return mainWindow;
            }
        }

        private VolatileSettings VolatileSettings
        {
            get { return this.SettingsService.VolatileSettings; }
        }

        public void RefreshTiles()
        {
            this.MainWindowViewModel.Refresh();
        }

        public void ShowHomePage()
        {
            MainWindowViewModel.NavigateToHomePage();

            if (!MainWindow.IsActive)
            {
                ShowMainWindow(true);
            }
        }

        public void ShowMainWindow(bool forceToForeground = false)
        {
            WindowUtilities.EnsureVisibleAndActive(MainWindow);
            if (forceToForeground)
            {
                WindowUtilities.ForceToForeground(MainWindow);
            }
        }

        public bool PromptShouldOpen(ViewModelBase ownerViewModel, int itemCount)
        {
            bool shouldOpen = true;

            if (itemCount > 5)
            {
                var result = this.MessageBoxService.Show(ownerViewModel, "You are about to open more than a few items at once, are you sure you want to continue?",
                    "Prompt", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel);

                shouldOpen = (result == MessageBoxResult.OK);
            }

            return shouldOpen;
        }

        public void ShowNewWorkItemWindow()
        {
            ShowMainWindow(true);
            MainWindowViewModel.NavigateToNewWorkItemPage();
        }

        public void ShowQuickSearchWindow()
        {
            if (quickSearchWindow == null)
            {
                var projectContext = this.SessionService.Session.ProjectContext;
                var project = (projectContext != null) ? projectContext.ProjectInfo : null;
                if (project != null)
                {
                    quickSearchWindow = new QuickSearchWindow(project);
                    quickSearchWindow.SearchTriggered += HandleQuickSearchTriggered;
                    quickSearchWindow.Deactivated += HandleQuickSearchWindowDeactivated;
                    quickSearchWindow.Closed += HandleQuickSearchWindowClosed;

                    quickSearchWindow.Reset();
                    WindowUtilities.EnsureVisibleAndActive(quickSearchWindow);
                    WindowUtilities.ForceToForeground(quickSearchWindow);
                }
            }
        }

        public void NavigateTo(object page)
        {
            MainWindow.ViewModel.Navigation.NavigateToPage(page);
        }

        public void ShowWorkItemWindow(WorkItemReference reference)
        {
            var factory = this.SessionService.Session.ProjectContext.HyperlinkFactory;
            var url = factory.GetWorkItemUrl(reference.Id);
            ExternalWebBrowser.Launch(url);

            this.TrackingService.RecentlyViewed(reference);
            this.HistoryService.History.WorkItemViews++;
        }

        public void ShowNewWorkItemWindow(WorkItemTypeReference typeReference)
        {
            // TODO: Can we refactor work item type references to have a stronger project name?
            // KLUDGE: Very kludgy getting of a project name, since WorkItemTypeReference does not have one (it has an id/project url from the old model)
            // NOTE that in principle, the work item type reference could be for a different project that is not the current project, so watch out
            var factory = this.SessionService.Session.ProjectContext?.HyperlinkFactory;
            if (factory == null)
            {
                throw new InvalidOperationException("Cannot create a work item type when there's no current project selected.");
            }

            var url = factory.GetNewWorkItemUrl(typeReference.Name);
            ExternalWebBrowser.Launch(url);
        }

        public void ShowNewWorkItemWindow(DefaultWorkItemInfo workItemInfo)
        {
            ShowNewWorkItemWindow(workItemInfo.WorkItemType);
        }

        /*
        // TODO: Keeping just in case we need to revert back to using TeamProjectPicker, and re-adding the ExtendedClient TFS Nuget
        public ICollection<ProjectInfo> ShowLegacyProjectPickerDialog(ViewModelBase ownerViewModel)
        {
            List<ProjectInfo> projects = new List<ProjectInfo>();
            TeamProjectPicker picker = new TeamProjectPicker(TeamProjectPickerMode.MultiProject, false);
            picker.AcceptButtonText = "Select";
            picker.Text = "Add one or more Team Projects";

            Window ownerWindow = (ownerViewModel != null) ? View.GetWindow(ownerViewModel) : null;
            var winFormsOwner = (ownerWindow != null) ? ownerWindow.GetWinFormsWin32Window() : null;
            var result = (winFormsOwner != null) ? picker.ShowDialog(winFormsOwner) : picker.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var selection in picker.SelectedProjects)
                {
                    ProjectReference pr = new ProjectReference(picker.SelectedTeamProjectCollection.Uri, new Uri(selection.Uri));
                    ProjectInfo pi = new ProjectInfo(pr, selection.Name);
                    projects.Add(pi);
                }
            }

            return projects;
        }
        */

        public ICollection<ProjectInfo> ShowProjectPickerDialog(ViewModelBase ownerViewModel)
        {
            var viewModel = ViewModelFactory.Create<ProjectPickerDialogViewModel>();
            ProjectPickerDialog dialog = new ProjectPickerDialog();
            dialog.DataContext = viewModel;
            dialog.Owner = ownerViewModel != null ? View.GetWindow(ownerViewModel) : null;
            var result = dialog.ShowDialog();

            List<ProjectInfo> projects = new List<ProjectInfo>();

            if (result == true)
            {
                foreach (var p in viewModel.SelectedProjects)
                {
                    projects.Add(new ProjectInfo(new ProjectReference(viewModel.SelectedProjectCollectionUrl, p.Id), p.Name));
                }
            }

            return projects;
        }

        public bool PromptSaveOnDirtyWindows()
        {
            bool anyCancel = false;
            foreach (IDirtiableWindow window in Windows.OfType<IDirtiableWindow>().ToArray())
            {
                anyCancel |= window.PromptSaveIfDirty();
            }

            return anyCancel;
        }

        public void ReleaseAll()
        {
            foreach (IDirtiableWindow window in Windows.OfType<IDirtiableWindow>().ToArray())
            {
                window.NoPrompt = true;
            }
        }

        public void ShowSettingsPage()
        {
            ShowMainWindow();

            MainWindow.ViewModel.NavigateToSettingsPage();
        }

        public void HideSettingsPage()
        {
            if (MainWindow.ViewModel.Navigation.CanGoBack)
            {
                MainWindow.ViewModel.Navigation.GoBack();
            }
        }

        public WorkItemQuerySelection ShowQueryPickerDialog(ViewModelBase ownerViewModel, ProjectContext projectContext)
        {
            WorkItemQueryPickerDialog dialog = new WorkItemQueryPickerDialog();

            QueryHierarchyItemViewModel viewModel = new QueryHierarchyItemViewModel(projectContext.WorkItemTrackingClient, projectContext.ProjectName);
            viewModel.Loaded += delegate (object sender, EventArgs e)
            {
                dialog.Picker.SelectAndFocusFirstItem();
            };

            dialog.DataContext = viewModel;
            dialog.Owner = View.GetWindow(ownerViewModel);
            var result = (dialog.ShowDialog() == true) ? dialog.Selection : null;
            if (result != null)
            {
                return new WorkItemQuerySelection
                {
                    Reference = new WorkItemQueryReference(projectContext.ProjectInfo.ProjectCollectionUri, result.Id),
                    Name = result.Name
                };
            }

            return null;
        }

        public CodeFlowQueryInfo ShowCodeFlowQueryEditorDialog(ViewModelBase ownerViewModel)
        {
            return this.ShowCodeFlowQueryEditorDialog(ownerViewModel, new CodeFlowQueryInfo());
        }

        public CodeFlowQueryInfo ShowCodeFlowQueryEditorDialog(ViewModelBase ownerViewModel, CodeFlowQueryInfo queryInfo)
        {
            CodeFlowQueryEditorDialog dialog = new CodeFlowQueryEditorDialog();
            CodeFlowPickerViewModel viewModel = ViewModelFactory.Create<CodeFlowPickerViewModel>();
            viewModel.QueryInfo = queryInfo;
            dialog.DataContext = viewModel;
            dialog.Owner = View.GetWindow(ownerViewModel);

            return (dialog.ShowDialog() == true) ? viewModel.QueryInfo : null;
        }

        public string ShowColorPickerDialog(ViewModelBase ownerViewModel, string initialColor)
        {
            WindowsForms.ColorDialog colorPicker = new WindowsForms.ColorDialog();
            // Sets the initial color select to the current tile background color.
            colorPicker.Color = ColorTranslator.FromHtml(initialColor);

            // If the user clicks OK, return that color, otherwise return initial color
            if (colorPicker.ShowDialog() == WindowsForms.DialogResult.OK)
            {
                return ColorTranslator.ToHtml(colorPicker.Color);
            }
            return initialColor;
        }

        public void MinimizeWindowForCapture(ViewModelBase viewModel)
        {
            Window window = View.GetWindow(viewModel);

            if (window != null)
            {
                using (ExtendedSystemParameters.DisableSystemAnimations())
                {
                    window.WindowState = WindowState.Minimized;
                }
            }
        }

        public void RestoreAndActivateWindow(ViewModelBase viewModel)
        {
            Window window = View.GetWindow(viewModel);

            if (window != null)
            {
                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }

                window.Activate();
            }
        }

        public void ShowWelcomeDialog()
        {
            WelcomeDialog dialog = new WelcomeDialog();
            dialog.ShowDialog();
        }

        public void ShowRequestRatingDialog()
        {
            CustomDialogViewModel dialogViewModel = new CustomDialogViewModel();
            dialogViewModel.Title = "We love feedback";
            dialogViewModel.Message =
                "It looks like you've been using TeamMate quite a bit. We could really use your feedback. " +
                "You can provide it in our Toolbox page, in the Rating section on the right-hand side.\n\n" +
                "This should only take a couple of clicks. Is this a good time?";

            var yesButton = dialogViewModel.AddDefaultButton("Sure");
            var noButton = dialogViewModel.AddButton("No Thanks");
            var laterButton = dialogViewModel.AddCancelButton("Not Now");

            CustomDialog dialog = new CustomDialog();
            dialog.DataContext = dialogViewModel;

            if (MainWindow.IsVisible)
            {
                dialog.Owner = MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                if (dialogViewModel.PressedButton == yesButton)
                {
                    this.HistoryService.History.LastRated = DateTime.Now;
                    this.ExternalWebBrowserService.RateApplication();
                }
                else if (dialogViewModel.PressedButton == noButton)
                {
                    this.HistoryService.History.NotInterestedInRating = true;
                }
            }
        }

        public void ShowRequestFeedbackDialog()
        {
            this.HistoryService.History.LastFeedbackProvidedOrPrompted = DateTime.Now;

            CustomDialogViewModel dialogViewModel = new CustomDialogViewModel();
            dialogViewModel.Title = "We love feedback";
            dialogViewModel.Message =
                "Seriously, we use it to prioritize features and make our users happy. " +
                "Looks like you've been using TeamMate quite a bit, so we would really value your opinion.\n\n" +
                "Is this a good time?";

            var voteButton = dialogViewModel.AddDefaultButton("Vote on Features");
            var suggestButton = dialogViewModel.AddButton("Suggest a Feature");
            var laterButton = dialogViewModel.AddCancelButton("Not Now");

            CustomDialog dialog = new CustomDialog();
            dialog.DataContext = dialogViewModel;

            if (MainWindow.IsVisible)
            {
                dialog.Owner = MainWindow;
            }

            if (dialog.ShowDialog() == true)
            {
                if (dialogViewModel.PressedButton == voteButton)
                {
                    this.ExternalWebBrowserService.VoteOnFeatures();
                }
                else if (dialogViewModel.PressedButton == suggestButton)
                {
                    this.ExternalWebBrowserService.SuggestFeature();
                }
            }
        }

        public void MonitorWithProgressDialog(TaskContext taskContext)
        {
            ProgressDialog.Show(taskContext);
        }

        private T FindWindow<T>(Func<T, bool> predicate) where T : Window
        {
            return Windows.OfType<T>().FirstOrDefault(predicate);
        }

        private void HandleQuickSearchWindowDeactivated(object sender, EventArgs e)
        {
            if (quickSearchWindow.IsVisible)
            {
                quickSearchWindow.Close();
            }
        }

        private void HandleQuickSearchWindowClosed(object sender, EventArgs e)
        {
            quickSearchWindow = null;
        }

        private void HandleQuickSearchTriggered(object sender, QuickSearchTriggeredEventArgs e)
        {
            quickSearchWindow.Close();

            this.MainWindowViewModel.Search(e.SearchText, true, true);
        }


        public void PeriodicallyRequestFeedback()
        {
            var history = this.HistoryService.History;

            if (DateTime.Now > history.NextTimeToRequestFeedback)
            {
                history.NextTimeToRequestFeedback += ApplicationHistory.IntervalBetweenFeedbackChecks;

                if (history.HasBeenUsedEnough)
                {
                    // Used it enough to rate it...
                    if (ShouldPromptForRating(history))
                    {
                        this.ShowRequestRatingDialog();
                    }
                    else if (ShouldPromptForFeedback(history))
                    {
                        this.ShowRequestFeedbackDialog();
                    }
                }
            }
        }

        private static bool ShouldPromptForFeedback(ApplicationHistory history)
        {
            // If feedback was never provided, or it was provided a long time ago, ask for it again
            // TODO: Maybe consider version changes from last time feedback was provided?
            return !history.NotInterestedInFeedback &&
                (history.LastFeedbackProvidedOrPrompted == null
                 || DateTime.Now > (history.LastFeedbackProvidedOrPrompted.Value + ApplicationHistory.IntervalBetweenFeedbackPrompts));
        }

        private static bool ShouldPromptForRating(ApplicationHistory history)
        {
            return !history.RatingEverProvided && !history.NotInterestedInRating;
        }

        public bool RequestShutdown()
        {
            bool shouldCancel = this.PromptSaveOnDirtyWindows();
            if (!shouldCancel)
            {
                // If the user said Yes, exit, but there are dirty work items, we will still see 
                try
                {
                    CleanupBeforeExit();
                }
                finally
                {
                    System.Windows.Application.Current.Shutdown();
                }
            }

            return !shouldCancel;
        }

        private void CleanupBeforeExit()
        {
            this.ReleaseAll();
        }
    }
}
