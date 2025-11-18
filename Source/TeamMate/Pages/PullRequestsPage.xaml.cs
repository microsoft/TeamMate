using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for PullRequestsPage.xaml
    /// </summary>
    [View(typeof(PullRequestPageViewModel))]
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class PullRequestsPage : UserControl, IResultsContainer
    {
        public PullRequestsPage()
        {
            View.Initialize(this);
            InitializeComponent();

            this.Loaded += HandlePageLoaded;
            this.Unloaded += HandlePageUnloaded;

            this.listView.ItemsActivated += HandleItemsActivated;
            this.listView.DragRequested += HandleDragRequested;
        }

        public bool SelectAndFocusFirstItem()
        {
            return this.listView.ListBox.SelectAndFocusFirstItem();
        }

        private void HandlePageLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.TextFilterApplied += HandleTextFilterApplied;
            }
        }

        private void HandlePageUnloaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.TextFilterApplied -= HandleTextFilterApplied;
            }
        }

        private void HandleTextFilterApplied(object sender, SearchExpression e)
        {
            SearchTextHighlighter.Highlight(this.listView, e);
        }

        private void HandleDragRequested(object sender, ListViewDragRequestedEventArgs e)
        {
            if (e.Items.Count == 1)
            {
                var pullRequest = e.Items.OfType<PullRequestRowViewModel>().FirstOrDefault();
                if (pullRequest != null)
                {
                    var dataObject = DataObjectFactory.CreateDraggableItem(pullRequest);
                    DragDrop.DoDragDrop(this, dataObject, DragDropEffects.All);
                }
            }
        }

        private void HandleItemsActivated(object sender, ListViewItemsActivatedEventArgs e)
        {
            var items = e.Items.OfType<PullRequestRowViewModel>().ToArray();
            ViewModel.OpenMany(items);
        }

        private PullRequestPageViewModel ViewModel
        {
            get { return this.DataContext as PullRequestPageViewModel; }
        }
    }
}
