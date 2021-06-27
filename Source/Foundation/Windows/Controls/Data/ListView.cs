// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// A custom control that implements advanced list box scenarios including list grouping,
    /// filtering, and sorting.
    /// </summary>
    /// <remarks>
    /// This custom control implements the Outlook list view control pattern.
    /// </remarks>
    public class ListView : Control
    {
        /// <summary>
        /// The command to expand all groups.
        /// </summary>
        public static readonly ICommand ExpandAll = new RoutedUICommand();

        /// <summary>
        /// The command to collapse all groups.
        /// </summary>
        public static readonly ICommand CollapseAll = new RoutedUICommand();

        /// <summary>
        /// The event that is fired when one or more list view items are activated.
        /// </summary>
        public static readonly RoutedEvent ListViewItemsActivatedEvent = EventManager.RegisterRoutedEvent(
            "ListViewItemsActivatedEvent", RoutingStrategy.Bubble, typeof(ListViewItemsActivatedEventHandler), typeof(ListView));

        public event ListViewItemsActivatedEventHandler ItemsActivated
        {
            add { AddHandler(ListViewItemsActivatedEvent, value); }
            remove { RemoveHandler(ListViewItemsActivatedEvent, value); }
        }

        private static readonly object OwnedHeaderContextMenuTag = new object();

        private IDisposable listBoxGroupMemento;

        private ListBox listBox;
        private FrameworkElement toggleSort;
        private FrameworkElement groupBy;
        private FrameworkElement header;

        // TODO: Why are some event handlers WPF style events, and others old school style event handlers?
        public event EventHandler ListBoxChanged;
        public event EventHandler<ListViewDragRequestedEventArgs> DragRequested;

        static ListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListView), new FrameworkPropertyMetadata(typeof(ListView)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListView"/> class.
        /// </summary>
        public ListView()
        {
            this.DataContextChanged += HandleDataContextChanged;
            this.CommandBindings.Add(ExpandAll, ExpandAllGroups);
            this.CommandBindings.Add(CollapseAll, CollapseAllGroups);
            this.Focusable = false;
        }

        /// <summary>
        /// Overridden to capture template parts.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.listBox = GetTemplateChild("PART_ListBox") as ListBox;
            if (this.listBox == null)
            {
                throw new NotSupportedException("A template must provide a ListBox part");
            }

            this.header = GetTemplateChild("PART_Header") as FrameworkElement;
            this.toggleSort = GetTemplateChild("PART_ToggleSort") as FrameworkElement;
            this.groupBy = GetTemplateChild("PART_GroupBy") as FrameworkElement;

            this.listBox.SelectionChanged += HandleListBoxSelectionChanged;
            this.listBox.MouseDoubleClick += HandleListBoxMouseDoubleClick;
            this.listBox.KeyDown += HandleListBoxKeyDown;
            GroupingViewUtilities.RegisterDefaultShortcuts(this.listBox);

            DragDropHelper dragDropHelper = DragDropHelper.Create(listBox);
            dragDropHelper.DragRequested += HandleDragRequested;

            if (this.toggleSort != null)
            {
                this.toggleSort.MouseLeftButtonDown += HandleSortButtonLeftButtonDown;
            }

            if (this.groupBy != null)
            {
                this.groupBy.MouseLeftButtonDown += HandleHeaderMouseLeftButtonDown;
            }

            InvalidateHeader();

            ListBoxChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets the view model.
        /// </summary>
        private ListViewModel ViewModel
        {
            get { return this.DataContext as ListViewModel; }
        }

        /// <summary>
        /// Gets the ListBox control hosted in this ListView.
        /// </summary>
        public ListBox ListBox
        {
            get { return this.listBox; }
        }

        /// <summary>
        /// The item context menu property.
        /// </summary>
        public static readonly DependencyProperty ItemContextMenuProperty = DependencyProperty.Register(
            "ItemContextMenu", typeof(ContextMenu), typeof(ListView)
        );

        /// <summary>
        /// Gets or sets the item context menu.
        /// </summary>
        public ContextMenu ItemContextMenu
        {
            get { return (ContextMenu)GetValue(ItemContextMenuProperty); }
            set { SetValue(ItemContextMenuProperty, value); }
        }

        /// <summary>
        /// The item template property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            "ItemTemplate", typeof(DataTemplate), typeof(ListView)
        );

        /// <summary>
        /// Gets or sets the item template.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        /// <summary>
        /// The item template selector property
        /// </summary>
        public static readonly DependencyProperty ItemTemplateSelectorProperty = DependencyProperty.Register(
            "ItemTemplateSelector", typeof(DataTemplateSelector), typeof(ListView)
        );

        /// <summary>
        /// Gets or sets the item template selector.
        /// </summary>
        public DataTemplateSelector ItemTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty); }
            set { SetValue(ItemTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Scrolls the list box to the top.
        /// </summary>
        public void ScrollToTop()
        {
            var scrollViewer = VisualTreeUtilities.FirstDescendantsOfType<ScrollViewer>(this.ListBox).FirstOrDefault();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(0);
            }
        }

        /// <summary>
        /// Collapses all groups in the list view.
        /// </summary>
        public void CollapseAllGroups()
        {
            GroupingViewUtilities.SetIsExpandedOnAllGroups(listBox, false);
        }

        /// <summary>
        /// Expands all groups in the list view.
        /// </summary>
        public void ExpandAllGroups()
        {
            GroupingViewUtilities.SetIsExpandedOnAllGroups(listBox, true);
        }

        /// <summary>
        /// Invalidates the header. If a header is available, registers header context menu
        /// items to match the current view model fields.
        /// </summary>
        private void InvalidateHeader()
        {
            ListViewModel viewModel = ViewModel;

            if (this.header != null && this.header.ContextMenu != null)
            {
                for (int i = 0; i < header.ContextMenu.Items.Count; i++)
                {
                    MenuItem item = header.ContextMenu.Items[i] as MenuItem;
                    if (item != null && item.Tag == OwnedHeaderContextMenuTag)
                    {
                        header.ContextMenu.Items.RemoveAt(i);
                        i--;
                    }
                }

                if (viewModel != null)
                {
                    for (int i = 0; i < viewModel.Fields.Count; i++)
                    {
                        MenuItem item = new MenuItem();
                        item.Tag = OwnedHeaderContextMenuTag;
                        item.IsCheckable = true;
                        item.DataContext = viewModel.Fields[i];
                        item.SetBinding(MenuItem.IsCheckedProperty, new Binding("IsSelected"));
                        item.SetBinding(MenuItem.HeaderProperty, new Binding("Name"));

                        header.ContextMenu.Items.Insert(i, item);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the collection view refreshing.
        /// </summary>
        private void HandleCollectionViewRefreshing(object sender, EventArgs e)
        {
            // TODO: capturing and restoring the group state was causing very weird and inconsistent
            // grouping animations that were a bug, we probably want to remove this code completely
            // listBoxGroupMemento = GroupingViewUtilities.CaptureGroupingState(listBox);
        }

        /// <summary>
        /// Handles the collection view refreshed.
        /// </summary>
        private void HandleCollectionViewRefreshed(object sender, EventArgs e)
        {
            if (listBoxGroupMemento != null)
            {
                // TODO: Inhibit animations when restoring?
                listBoxGroupMemento.Dispose();
                listBoxGroupMemento = null;
            }
        }

        /// <summary>
        /// Handles the data context changed.
        /// </summary>
        private void HandleDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ListViewModel oldModel = e.OldValue as ListViewModel;
            ListViewModel newModel = e.NewValue as ListViewModel;

            if (oldModel != null)
            {
                oldModel.CollectionViewRefreshing -= HandleCollectionViewRefreshing;
                oldModel.CollectionViewRefreshed -= HandleCollectionViewRefreshed;
            }

            if (newModel != null)
            {
                newModel.CollectionViewRefreshing += HandleCollectionViewRefreshing;
                newModel.CollectionViewRefreshed += HandleCollectionViewRefreshed;
            }

            InvalidateHeader();
        }

        /// <summary>
        /// Handles the ListBox mouse double click.
        /// </summary>
        private void HandleListBoxMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                object item = VisualTreeUtilities.GetListBoxItemAt<object>(e);
                if (item != null)
                {
                    var activatedEvent = new ListViewItemsActivatedEventArgs(new object[] { item });
                    RaiseEvent(activatedEvent);
                    e.Handled = activatedEvent.Handled;
                }
            }
        }

        /// <summary>
        /// Handles the ListBox key down.
        /// </summary>
        private void HandleListBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (listBox.SelectedItems.Count > 0)
                {
                    var activatedEvent = new ListViewItemsActivatedEventArgs(listBox.SelectedItems.OfType<object>().ToArray());
                    RaiseEvent(activatedEvent);
                    e.Handled = activatedEvent.Handled;
                }
            }
        }

        /// <summary>
        /// Handles the sort button left button down.
        /// </summary>
        private void HandleSortButtonLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.ToggleSort();
            e.Handled = true;
        }

        /// <summary>
        /// Handles the header mouse left button down.
        /// </summary>
        private void HandleHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (this.header != null && this.header.ContextMenu != null)
            {
                ContextMenu contextMenu = this.header.ContextMenu;
                contextMenu.PlacementTarget = (UIElement)sender;
                contextMenu.Placement = PlacementMode.MousePoint;
                contextMenu.IsOpen = true;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Handles the drag requested.
        /// </summary>
        private void HandleDragRequested(object sender, MouseButtonEventArgs e)
        {
            if (DragRequested != null)
            {
                object item = VisualTreeUtilities.GetListBoxItemAt<object>(e);
                if (item != null && listBox.SelectedItems.Contains(item))
                {
                    DragRequested(this, new ListViewDragRequestedEventArgs(e, listBox.SelectedItems.OfType<object>().ToArray()));
                }
            }
        }


        /// <summary>
        /// Handles the ListBox selection changed.
        /// </summary>
        private void HandleListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Select(listBox.SelectedItems.OfType<object>());
            }
        }
    }

    /// <summary>
    /// A delegate that is invoked when list view items are activated (via ENTER or mouse double click).
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The <see cref="ListViewItemsActivatedEventArgs"/> instance containing the event data.</param>
    public delegate void ListViewItemsActivatedEventHandler(object sender, ListViewItemsActivatedEventArgs e);

    /// <summary>
    /// The event arguments for an activated event.
    /// </summary>
    public class ListViewItemsActivatedEventArgs : RoutedEventArgs
    {
        public ListViewItemsActivatedEventArgs(ICollection<object> items)
            : base(ListView.ListViewItemsActivatedEvent)
        {
            this.Items = items;
        }

        public ICollection<object> Items { get; private set; }
    }

    /// <summary>
    /// The drag request arguemnts for a drag request event.
    /// </summary>
    public class ListViewDragRequestedEventArgs : EventArgs
    {
        public ListViewDragRequestedEventArgs(MouseButtonEventArgs sourceEvent, ICollection<object> items)
        {
            this.SourceEvent = sourceEvent;
            this.Items = items;
        }

        public MouseButtonEventArgs SourceEvent { get; set; }
        public ICollection<object> Items { get; private set; }
    }
}
