using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for CodeFlowReviewsPage.xaml
    /// </summary>
    [View(typeof(CodeFlowReviewsPageViewModel))]
    public partial class CodeFlowReviewsPage : UserControl, IResultsContainer
    {
        public CodeFlowReviewsPage()
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
                var summary = e.Items.OfType<CodeFlowReviewViewModel>().Select(review => review.Summary).FirstOrDefault();
                if (summary != null)
                {
                    var dataObject = DataObjectFactory.CreateDraggableItem(summary);
                    DragDrop.DoDragDrop(this, dataObject, DragDropEffects.All);
                }
            }
        }

        private void HandleItemsActivated(object sender, ListViewItemsActivatedEventArgs e)
        {
            var items = e.Items.OfType<CodeFlowReviewViewModel>().ToArray();
            ViewModel.OpenMany(items);
        }

        private CodeFlowReviewsPageViewModel ViewModel
        {
            get { return this.DataContext as CodeFlowReviewsPageViewModel; }
        }
    }
}
