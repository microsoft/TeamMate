using Microsoft.Internal.Tools.TeamMate.Foundation.Windows;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Internal.Tools.TeamMate.Utilities;
using Microsoft.Internal.Tools.TeamMate.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Internal.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for WorkItemsPage.xaml
    /// </summary>
    [View(typeof(WorkItemsPageViewModel))]
    public partial class WorkItemsPage : UserControl, IResultsContainer
    {
        public WorkItemsPage()
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
            var workItems = e.Items.OfType<WorkItemRowViewModel>().Select(wii => wii.WorkItem).ToArray();
            if (workItems.Any())
            {
                var factory = ViewModel.HyperlinkFactory;
                var dataObject = DataObjectFactory.CreateDraggableItem(workItems, factory);
                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.All);
            }
        }

        private void HandleItemsActivated(object sender, ListViewItemsActivatedEventArgs e)
        {
            var items = e.Items.OfType<WorkItemRowViewModel>().ToArray();
            ViewModel.OpenMany(items);
        }

        private WorkItemsPageViewModel ViewModel
        {
            get { return this.DataContext as WorkItemsPageViewModel; }
        }
    }
}
