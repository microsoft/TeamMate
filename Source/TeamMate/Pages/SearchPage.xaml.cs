using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using Microsoft.Tools.TeamMate.Services;
using Microsoft.Tools.TeamMate.Utilities;
using Microsoft.Tools.TeamMate.ViewModels;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Pages
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    [View(typeof(SearchPageViewModel))]
    public partial class SearchPage : UserControl, IResultsContainer
    {
        private ContextMenu placeholderItemContextMenu;

        public SearchPage()
        {
            InitializeComponent();

            View.Initialize(this);

            this.DataContextChanged += HandleDataContextChanged;

            this.placeholderItemContextMenu = new ContextMenu();

            this.listView.ItemTemplateSelector = DelegateFactory.CreateTemplateSelector(SelectTemplate);
            this.listView.ItemContextMenu = placeholderItemContextMenu;
            this.listView.ItemsActivated += HandleItemsActivated;
            this.listView.DragRequested += HandleDragRequested;

            this.attentionIcon.MouseLeftButtonDown += HandleAttentionIconClick;

            this.Loaded += HandleLoaded;
        }

        private void HandleLoaded(object sender, RoutedEventArgs e)
        {
            this.listView.ApplyTemplate();
            this.ListBox.ContextMenuOpening += HandleContextMenuOpening;
        }

        private void HandleContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Replacing the full context menu on open, based on http://msdn.microsoft.com/en-us/library/bb613568.aspx
            SearchResult searchResult = VisualTreeUtilities.GetListBoxItemAt<SearchResult>(e);
            if (searchResult != null)
            {
                // Suppress display of placeholder context menu item
                e.Handled = true;


                // Displaying context menu for a list box item
                var selectedItems = this.ListBox.SelectedItems.OfType<SearchResult>();

                ContextMenu replacementContextMenu = null;

                if (selectedItems.All(sr => sr.Item is WorkItemRowViewModel))
                {
                    replacementContextMenu = this.FindResource<ContextMenu>("WorkItemContextMenu");
                }
                else if (selectedItems.All(sr => sr.Item is PullRequestViewModel))
                {
                    replacementContextMenu = this.FindResource<ContextMenu>("PullRequestContextMenu");
                }
                else
                {
                    // A mix of results we cannot handle gracefully... Show no context menu here
                }

                // There is a replacement context menu, open it
                if (replacementContextMenu != null)
                {
                    replacementContextMenu.PlacementTarget = (UIElement)e.Source;
                    replacementContextMenu.IsOpen = true;
                }
            }
        }

        private ListBox ListBox
        {
            get
            {
                return this.listView.ListBox;
            }
        }

        public bool SelectAndFocusFirstItem()
        {
            return this.ListBox.SelectAndFocusFirstItem();
        }

        private void HandleAttentionIconClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.ShowErrorDetails();
            e.Handled = true;
        }

        private void HandleItemsActivated(object sender, ListViewItemsActivatedEventArgs e)
        {
            var items = e.Items.OfType<SearchResult>().ToArray();
            ViewModel.OpenMany(items);
        }

        private void HandleDragRequested(object sender, ListViewDragRequestedEventArgs e)
        {
            object dataObject = null;

            var items = e.Items.OfType<SearchResult>().Select(sr => sr.Item).ToArray();
            if (items.All(i => i is WorkItemRowViewModel))
            {
                var workItems = items.OfType<WorkItemRowViewModel>();
                if (workItems.All(wi => wi.WorkItem != null))
                {
                    var factory = ViewModel.HyperlinkFactory;
                    dataObject = DataObjectFactory.CreateDraggableItem(workItems.Select(wi => wi.WorkItem).ToArray(), factory);
                }
                else
                {
                    dataObject = DataObjectFactory.CreateDraggableItem(workItems.Select(wi => wi.Reference).ToArray());
                }
            }
            else if (items.Length == 1 && items.First() is PullRequestViewModel)
            {
                // TODO(MEM)
              //  PullRequestViewModel singleReview = (PullRequestViewModel)items.First();
              //  dataObject = DataObjectFactory.CreateDraggableItem(singleReview.Summary);
            }

            if (dataObject != null)
            {
                DragDrop.DoDragDrop(this, dataObject, DragDropEffects.All);
            }
        }

        private DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            DataTemplate template = null;

            SearchResult result = item as SearchResult;
            item = (result != null) ? result.Item : null;

            if (item is WorkItemRowViewModel)
            {
                template = this.FindResource<DataTemplate>("WorkItemTemplate");
            }
            else if (item is PullRequestViewModel)
            {
                template = this.FindResource<DataTemplate>("CodeReviewTemplate");
            }

            return template;
        }

        public DataTemplateSelector TemplateSelector { get; private set; }

        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SearchPageViewModel viewModel = ViewModel;
            viewModel.SearchResultsChanged += HandleSearchResultsChanged;
            viewModel.SearchStarted += HandleSearchStarted;
            viewModel.SearchCompleted += HandleSearchCompleted;
        }

        private void HandleSearchStarted(object sender, EventArgs e)
        {
            this.listView.ScrollToTop();
        }

        private void HandleSearchResultsChanged(object sender, EventArgs e)
        {
            HighlightText();
        }

        private void HandleSearchCompleted(object sender, EventArgs e)
        {
        }

        private void HighlightText()
        {
            SearchTextHighlighter.Highlight(this.listView, ViewModel.SearchExpression);
        }

        private SearchPageViewModel ViewModel
        {
            get
            {
                return this.DataContext as SearchPageViewModel;
            }
        }
    }
}
