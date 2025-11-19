using Microsoft.Tools.TeamMate.Controls;
using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Input;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Pages;
using Microsoft.Tools.TeamMate.Resources;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class MainWindow
    {
        private const int FilterApplyDelayInMs = 400;

        public MainWindow()
        {
            InitializeComponent();
            View.Initialize(this);

            RegisterBindings();

            this.searchControl.PreviewKeyDown += HandleSearchControlKeyDown;
            this.searchControl.SearchTriggered += HandleSearchTriggered;
            this.searchControl.IncrementalSearchTriggered += HandleIncrementalSearchTriggered;

            this.DataContextChanged += HandleDataContextChanged;

            InvalidateSearchControlState();
        }

        public ViewService ViewService
        {
            get { return this.navigationFrame.ViewService; }
            set { this.navigationFrame.ViewService = value; }
        }

        private void ToggleToolBarWidth()
        {
            var width = this.toolbar.Width;
            if (width != 255)
            {
                this.toolbar.Width = 255;
            }
            else
            {
                this.toolbar.Width = 48;
            }
        }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var oldModel = e.OldValue as MainWindowViewModel;
            var newModel = e.NewValue as MainWindowViewModel;

            if (oldModel != null)
            {
                oldModel.PropertyChanged -= HandleViewModelPropertyChanged;
            }

            if (newModel != null)
            {
                newModel.PropertyChanged += HandleViewModelPropertyChanged;
            }
        }

        private void HandleViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPageSupportsFiltering")
            {
                InvalidateSearchControlState();
            }
        }

        private void InvalidateSearchControlState()
        {
            bool supportsFiltering = (ViewModel != null && ViewModel.CurrentPageSupportsFiltering);

            string hintText = KeyGestureUtilities.FormatShortcut("Search", TeamMateGestures.Search);

            if (supportsFiltering)
            {
                hintText = String.Format("{0} or {1}", hintText, KeyGestureUtilities.FormatShortcut("Filter", TeamMateGestures.Filter));
            }

            this.searchControl.HintText = hintText;
            this.searchControl.SupportsFiltering = supportsFiltering;
        }

        private void HandleSearchControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                IResultsContainer resultsContainer = this.navigationFrame.CurrentView as IResultsContainer;
                if (resultsContainer != null)
                {
                    e.Handled = resultsContainer.SelectAndFocusFirstItem();
                }
            }
        }

        private void HandleSearchTriggered(object sender, SearchTriggeredEventArgs e)
        {
            if (this.searchControl.Mode == SearchBoxMode.Search)
            {
                ViewModel.PerformSearch();
            }
            else
            {
                TryApplyFilter(true);
            }
        }

        private CancellationTokenSource previousFilterRequestTokenSource;

        private async void TryApplyFilter(bool immediate = false)
        {
            if (this.searchControl.Mode == SearchBoxMode.Filter)
            {
                IFilterable filterable = ViewModel.CurrentPage as IFilterable;
                if (filterable != null)
                {
                    // Cancel a previous request that was pending... Then queue this.
                    if (previousFilterRequestTokenSource != null)
                    {
                        previousFilterRequestTokenSource.Cancel();
                        previousFilterRequestTokenSource = null;
                    }

                    previousFilterRequestTokenSource = new CancellationTokenSource();
                    var cancellationToken = previousFilterRequestTokenSource.Token;

                    // If search text is cleared, also apply immediately, otherwise queue
                    string searchText = ViewModel.SearchText;
                    immediate |= String.IsNullOrWhiteSpace(searchText);

                    try
                    {
                        // Only queue a filter apply request every 
                        if (!immediate)
                        {
                            await Task.Delay(FilterApplyDelayInMs, cancellationToken);
                        }

                        if (cancellationToken.IsCancellationRequested)
                        {
                            return;
                        }

                        filterable.ApplyTextFilter(searchText);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
            }
        }

        private void HandleIncrementalSearchTriggered(object sender, IncrementalSearchTriggeredEventArgs e)
        {
            TryApplyFilter();
        }

        private void RegisterBindings()
        {
            this.CommandBindings.Add(TeamMateCommands.Hamburger, this.ToggleToolBarWidth);

            RoutedUICommand newItemCommand = new RoutedUICommand();
            newItemCommand.InputGestures.Add(TeamMateGestures.New);
            this.CommandBindings.Add(newItemCommand, NewWorkItem);

            RoutedUICommand stopFilteringOrMinimize = new RoutedUICommand();
            stopFilteringOrMinimize.InputGestures.Add(TeamMateGestures.StopFilteringOrMinimize);
            this.CommandBindings.Add(stopFilteringOrMinimize, StopFilteringOrMinimize);

            RoutedUICommand searchCommand = new RoutedUICommand();
            searchCommand.InputGestures.Add(TeamMateGestures.Search);
            this.CommandBindings.Add(searchCommand, FocusSearchBoxInSearchMode);

            RoutedUICommand filterCommand = new RoutedUICommand();
            filterCommand.InputGestures.Add(TeamMateGestures.Filter);
            this.CommandBindings.Add(filterCommand, FocusSearchBoxInFilterMode);
        }

        private void StopFilteringOrMinimize()
        {
            // If filtering, clearing filter, otherwise minimize
            if (ViewModel.CurrentPageSupportsFiltering
                && this.searchControl.Mode == SearchBoxMode.Filter && this.searchControl.HasSearchText)
            {
                this.searchControl.SearchText = null;
            }
            else
            {
                this.HideWindow();
            }
        }

        private void FocusSearchBoxInSearchMode(object sender, ExecutedRoutedEventArgs e)
        {
            this.searchControl.Mode = SearchBoxMode.Search;
            this.searchControl.FocusTextBox();
        }

        private void FocusSearchBoxInFilterMode()
        {
            if (ViewModel.CurrentPageSupportsFiltering)
            {
                this.searchControl.Mode = SearchBoxMode.Filter;
                this.searchControl.FocusTextBox();
            }
        }

        private void NewWorkItem()
        {
            ViewModel.QuickCreateDefault();
        }

        public MainWindowViewModel ViewModel
        {
            get { return DataContext as MainWindowViewModel; }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            e.Cancel = true;
            this.HideWindow();
        }

        public void HideWindow()
        {
            this.Hide();
            ViewModel.Hide();
        }
    }
}
