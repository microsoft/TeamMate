using Microsoft.Tools.TeamMate.Foundation.Windows;
using Microsoft.Tools.TeamMate.Foundation.Windows.Controls;
using Microsoft.Tools.TeamMate.ViewModels;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Controls
{
    /// <summary>
    /// Interaction logic for QueryPickerControl.xaml
    /// </summary>
    [SupportedOSPlatform("windows10.0.19041.0")]
    public partial class QueryPickerControl : UserControl
    {
        public static readonly DependencyProperty HasSelectedQueryProperty = DependencyProperty.Register(
            "HasSelectedQuery", typeof(bool), typeof(QueryPickerControl)
        );

        public static readonly DependencyProperty SelectedQueryProperty = DependencyProperty.Register(
            "SelectedQuery", typeof(QueryHierarchyItem), typeof(QueryPickerControl)
        );

        public event EventHandler ItemActivated;

        public QueryPickerControl()
        {
            // Initialize this before InitializeComponent, as the binding will take effect at that time
            this.ItemTemplateSelector = DelegateFactory.CreateTemplateSelector(this.SelectTemplate);

            InitializeComponent();

            this.Loaded += OnLoaded;
            this.DataContextChanged += OnDataContextChanged;

            this.treeView.SelectedItemChanged += HandleSelectedItemChanged;
            this.treeView.MouseDoubleClick += HandleMouseDoubleClick;

        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.EnsureLoaded();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.EnsureLoaded();
        }

        private async void EnsureLoaded()
        {
            if (this.IsLoaded)
            {
                TreeItemViewModelBase rootItem = this.DataContext as TreeItemViewModelBase;
                if (rootItem != null)
                {
                    await rootItem.EnsureLoadedAsync();
                }
            }
        }

        public DataTemplateSelector ItemTemplateSelector { get; private set; }

        private DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            string resourceName = "TreeItemTemplate";
            TreeItemViewModelBase viewModel = item as TreeItemViewModelBase;
            if (viewModel != null)
            {
                if (viewModel.TreeNodeType == TreeNodeType.Loading)
                {
                    resourceName = "LoadingTemplate";
                }
                else if (viewModel.TreeNodeType == TreeNodeType.Error)
                {
                    resourceName = "ErrorTemplate";
                }
            }

            return this.FindResource<DataTemplate>(resourceName);
        }


        public void SelectAndFocusFirstItem()
        {
            treeView.SelectAndFocusFirstItem();
        }

        private void HandleMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && HasSelectedQuery)
            {
                if (ItemActivated != null)
                {
                    e.Handled = true;
                    ItemActivated(this, EventArgs.Empty);
                }
            }
        }

        public QueryHierarchyItem SelectedQuery
        {
            get { return (QueryHierarchyItem)GetValue(SelectedQueryProperty); }
            private set { SetValue(SelectedQueryProperty, value); }
        }

        public bool HasSelectedQuery
        {
            get { return (bool)GetValue(HasSelectedQueryProperty); }
            private set { SetValue(HasSelectedQueryProperty, value); }
        }

        private void HandleSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedViewModel = treeView.SelectedItem as QueryHierarchyItemViewModel;
            var item = (selectedViewModel != null) ? selectedViewModel.Item : null;
            SelectedQuery = (item != null && item.IsFolder != true) ? item : null;
            HasSelectedQuery = (SelectedQuery != null);
        }
    }
}
