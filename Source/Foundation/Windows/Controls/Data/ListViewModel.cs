// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// The view model for the ListView control.
    /// </summary>
    public class ListViewModel : ViewModelBase
    {
        private const string DefaultEmptyCollectionText = "We didn't find anything to show here.";
        private const string DefaultEmptyFilteredCollectionText = "We couldn't find what you were looking for.";

        private string noItemsText;
        private Predicate<object> searchFilter;
        private ICollectionView collectionView;
        private bool showInGroups;
        private ListFieldInfo orderByField;
        private string sortOrderText;
        private ObservableCollection<ListFieldInfo> fields = new ObservableCollection<ListFieldInfo>();
        private ObservableCollection<ListViewFilter> filters = new ObservableCollection<ListViewFilter>();
        private List<object> selectedItems = new List<object>();

        private string emptyFilteredCollectionText;
        private string emptyCollectionText;
        private bool hasFilters;
        private bool hasFields;
        private bool isFilterByVisible;
        private bool isGroupByVisible;
        private SortDescription defaultSortDescription;

        public event EventHandler CollectionViewRefreshing;
        public event EventHandler CollectionViewRefreshed;
        public event EventHandler SelectionChanged;
        public event EventHandler FilterApplied;
        public event EventHandler OrderByFieldChanged;
        public event EventHandler FilterByFieldChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewModel"/> class.
        /// </summary>
        public ListViewModel()
        {
            this.filters.CollectionChanged += HandleFiltersCollectionChanged;
            this.fields.CollectionChanged += HandleFieldsCollectionChanged;
            this.selectedItems = new List<object>();
            this.isGroupByVisible = true;
            this.isFilterByVisible = true;
            this.emptyCollectionText = DefaultEmptyCollectionText;
            this.emptyFilteredCollectionText = DefaultEmptyFilteredCollectionText;
            this.PropertyChanged += HandleOrderByFieldChanged;

            foreach (var filter in this.filters)
            {
                filter.PropertyChanged += HandleFilterByFieldChanged;
            }

            InvalidateNoItemsText();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ListViewModel"/> class.
        /// </summary>
        /// <param name="collectionView">The collection view.</param>
        public ListViewModel(ICollectionView collectionView)
            : this()
        {
            this.CollectionView = collectionView;
        }

        /// <summary>
        /// Gets the text to dislay when no items are available.
        /// </summary>
        public string NoItemsText
        {
            get { return this.noItemsText; }
            private set { SetProperty(ref this.noItemsText, value); }
        }

        /// <summary>
        /// Gets or sets the text displayed when the collection has been filtered and no matching items are found.
        /// </summary>
        public string EmptyFilteredCollectionText
        {
            get { return this.emptyFilteredCollectionText; }
            set
            {
                if (SetProperty(ref this.emptyFilteredCollectionText, value))
                {
                    InvalidateNoItemsText();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text displayed when the collection is empty.
        /// </summary>
        public string EmptyCollectionText
        {
            get { return this.emptyCollectionText; }
            set
            {
                if (SetProperty(ref this.emptyCollectionText, value))
                {
                    InvalidateNoItemsText();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the list view has any filters.
        /// </summary>
        public bool HasFilters
        {
            get { return this.hasFilters; }
            private set { SetProperty(ref this.hasFilters, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the list view has any fields.
        /// </summary>
        public bool HasFields
        {
            get { return this.hasFields; }
            private set { SetProperty(ref this.hasFields, value); }
        }

        /// <summary>
        /// Gets or sets the search filter for the items in this collection. This can be an arbitrary
        /// predicate to filter items.
        /// </summary>
        public Predicate<object> SearchFilter
        {
            get { return this.searchFilter; }
            set
            {
                if (SetProperty(ref this.searchFilter, value))
                {
                    InvalidateNoItemsText();
                    InvalidateFilters();
                }
            }
        }

        /// <summary>
        /// Gets the selected items.
        /// </summary>
        public ICollection<object> SelectedItems
        {
            get { return this.selectedItems; }
        }

        /// <summary>
        /// Gets a value indicating whether the list has any selected items.
        /// </summary>
        public bool HasSelectedItems
        {
            get { return this.selectedItems.Any(); }
        }

        /// <summary>
        /// Gets a single selected item, or <c>null</c> if no selection is made.
        /// </summary>
        public object SingleSelectedItem
        {
            get { return (HasSingleSelectedItem) ? this.selectedItems.FirstOrDefault() : null; }
        }

        /// <summary>
        /// Gets a value indicating whether there is a single selected item.
        /// </summary>
        public bool HasSingleSelectedItem
        {
            get { return this.selectedItems.Count == 1; }
        }

        /// <summary>
        /// Sets the selection to one or more items. (clears the selection if null or empty)
        /// </summary>
        /// <param name="selection">The selection.</param>
        public void Select(IEnumerable<object> selection)
        {
            bool noUpdates = (selection == null || !selection.Any()) && !this.selectedItems.Any();

            if (!noUpdates)
            {
                this.selectedItems.Clear();

                if (selection != null)
                {
                    this.selectedItems.AddRange(selection);
                }

                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Gets or sets the collection view containing the items displayed in this list view.
        /// </summary>
        public ICollectionView CollectionView
        {
            get { return this.collectionView; }
            set { SetProperty(ref this.collectionView, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the list view shoudl show items in groups.
        /// </summary>
        public bool ShowInGroups
        {
            get { return this.showInGroups; }
            set
            {
                if (SetProperty(ref this.showInGroups, value))
                {
                    if (CollectionView != null && OrderByField != null)
                    {
                        DeferredInvalidateGroups();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the filter by UX should be visible or not.
        /// </summary>
        public bool IsFilterByVisible
        {
            get { return this.isFilterByVisible; }
            set { SetProperty(ref this.isFilterByVisible, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the group by UX should be visible or not.
        /// </summary>
        public bool IsGroupByVisible
        {
            get { return this.isGroupByVisible; }
            set { SetProperty(ref this.isGroupByVisible, value); }
        }

        /// <summary>
        /// Gets or sets the default sort description.
        /// </summary>
        public SortDescription DefaultSortDescription
        {
            get { return this.defaultSortDescription; }
            set { SetProperty(ref this.defaultSortDescription, value); }
        }

        /// <summary>
        /// Gets the current field by which the list is being ordered (or null if not ordered yet).
        /// </summary>
        public ListFieldInfo OrderByField
        {
            get { return this.orderByField; }
            private set { SetProperty(ref this.orderByField, value); }
        }

        /// <summary>
        /// Gets or sets the sort order text.
        /// </summary>
        public string SortOrderText
        {
            get { return this.sortOrderText; }
            set { SetProperty(ref this.sortOrderText, value); }
        }

        /// <summary>
        /// Gets the available fields for this list.
        /// </summary>
        public ObservableCollection<ListFieldInfo> Fields
        {
            get { return this.fields; }
        }

        /// <summary>
        /// Gets the available filters for this list.
        /// </summary>
        public ObservableCollection<ListViewFilter> Filters
        {
            get { return this.filters; }
        }

        /// <summary>
        /// Orders the list by the given field name.
        /// </summary>
        /// <param name="fieldName">The field name as a string.</param>
        public void OrderByFieldName(String fieldName)
        {
            if (!String.IsNullOrEmpty(fieldName))
            {
                var field = this.Fields.Where(x => x.PropertyName == fieldName).FirstOrDefault();
                if (field != null)
                {
                    this.OrderBy(field);
                }
            }
        }

        /// <summary>
        /// Orders the list by the given filter name.
        /// </summary>
        /// <param name="filter">The filter as a string.</param>
        public void FilterByFieldName(String filterName)
        {
            if (!String.IsNullOrEmpty(filterName))
            {
                var filter = this.Filters.Where(x => x.Name == filterName).FirstOrDefault();
                if (filter != null)
                {
                    filter.IsSelected = true;
                }
            }
        }

        /// <summary>
        /// Orders the list by the given field.
        /// </summary>
        /// <param name="field">The field.</param>
        public void OrderBy(ListFieldInfo field)
        {
            Assert.ParamIsNotNull(field, "field");

            OrderBy(field, field.DefaultSortDirection);
        }

        /// <summary>
        /// Orders the list by the given field in the given direction.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="direction">The direction.</param>
        public void OrderBy(ListFieldInfo field, ListSortDirection direction)
        {
            Assert.ParamIsNotNull(field, "field");

            if (!fields.Contains(field))
            {
                throw new ArgumentException("The order by field needs to be contained in the list of fields");
            }

            field.IsSelected = true;

            OrderByField = field;

            WithDeferredRefresh(delegate()
            {
                var sort = new SortDescription(field.PropertyName, direction);
                Sort(sort);

                // TODO: The group might not have changed from before, only the sort order, we should avoid invalidating groups in that case...
                InvalidateGroups();
            });

            SortOrderText = (direction == ListSortDirection.Ascending) ? OrderByField.AscendingOrderText : OrderByField.DescendingOrderText;
        }

        /// <summary>
        /// Toggles the sort direction of the current order by field.
        /// </summary>
        public void ToggleSort()
        {
            if (OrderByField != null && CollectionView.SortDescriptions.Any())
            {
                var currentSort = CollectionView.SortDescriptions[0];
                var newDirection = (currentSort.Direction == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending;
                OrderBy(OrderByField, newDirection);
            }
        }

        /// <summary>
        /// Invalidates the no items text.
        /// </summary>
        private void InvalidateNoItemsText()
        {
            string text = (SearchFilter != null) ? EmptyFilteredCollectionText : EmptyCollectionText;
            NoItemsText = text;
        }

        /// <summary>
        /// Invalidates the filters.
        /// </summary>
        private void InvalidateFilters()
        {
            var filter = BuildCompoundFilter();
            if (CollectionView.Filter != filter)
            {
                WithDeferredRefresh(delegate()
                {
                    CollectionView.Filter = filter;
                });

                FilterApplied?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Builds the compound filter, including all of the applied filters and the search filter (if available).
        /// This is the ultimate predicate used to filter the collection view.
        /// </summary>
        private Predicate<object> BuildCompoundFilter()
        {
            CompoundFilter<object> compoundFilter = new CompoundFilter<object>();

            var filter = filters.FirstOrDefault(f => f.IsSelected);
            if (filter != null && filter.Predicate != null)
            {
                compoundFilter.Predicates.Add(filter.Predicate);
            }

            if (SearchFilter != null)
            {
                compoundFilter.Predicates.Add(SearchFilter);
            }

            var predicate = (compoundFilter.Predicates.Any()) ? compoundFilter.Filter : (Predicate<object>)null;
            return predicate;
        }

        /// <summary>
        /// Invalidates the list groups with a deferred refresh.
        /// </summary>
        private void DeferredInvalidateGroups()
        {
            WithDeferredRefresh(InvalidateGroups);
        }

        /// <summary>
        /// Invalidates the list groups.
        /// </summary>
        private void InvalidateGroups()
        {
            CollectionView.GroupDescriptions.Clear();

            if (ShowInGroups && OrderByField != null)
            {
                var pgd = (OrderByField.GroupConverter != null) ? new PropertyGroupDescription(OrderByField.PropertyName, OrderByField.GroupConverter)
                                                                : new PropertyGroupDescription(OrderByField.PropertyName);
                CollectionView.GroupDescriptions.Add(pgd);
            }
        }

        /// <summary>
        /// Sorts the list with a deferred refresh.
        /// </summary>
        /// <param name="sortDescription">The sort description.</param>
        private void DeferredRefreshSort(SortDescription sortDescription)
        {
            WithDeferredRefresh(delegate()
            {
                Sort(sortDescription);
            });
        }

        /// <summary>
        /// Sorts the list.
        /// </summary>
        /// <param name="sortDescription">The sort.</param>
        private void Sort(SortDescription sortDescription)
        {
            CollectionView.SortDescriptions.Clear();
            CollectionView.SortDescriptions.Add(sortDescription);

            // Struct, so we have to compare an empty name for the default value (kludge if you ask me)
            if (!String.IsNullOrEmpty(DefaultSortDescription.PropertyName))
            {
                CollectionView.SortDescriptions.Add(DefaultSortDescription);
            }
        }

        /// <summary>
        /// Performs an action with a deferred refresh, firing refreshing and refreshed events
        /// after the action is performed.
        /// </summary>
        /// <param name="action">The action.</param>
        private void WithDeferredRefresh(Action action)
        {
            CollectionViewRefreshing?.Invoke(this, EventArgs.Empty);

            using (CollectionView.DeferRefresh())
            {
                action();
            }

            CollectionViewRefreshed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Handles the fields collection changed.
        /// </summary>
        private void HandleFieldsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ListFieldInfo item in e.OldItems)
                {
                    item.PropertyChanged -= HandleFieldPropertyChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (ListFieldInfo item in e.NewItems)
                {
                    item.PropertyChanged += HandleFieldPropertyChanged;
                }
            }

            HasFields = fields.Any();
        }

        /// <summary>
        /// Handles the order by field changed.
        /// </summary>
        private void HandleOrderByFieldChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "OrderByField") && (OrderByFieldChanged != null))
            {
                OrderByFieldChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the filter by field changed.
        /// </summary>
        private void HandleFilterByFieldChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == "IsSelected") && (FilterByFieldChanged != null))
            {
                FilterByFieldChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Handles the filters collection changed.
        /// </summary>
        private void HandleFiltersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ListViewFilter item in e.OldItems)
                {
                    item.PropertyChanged -= HandleFilterSelectionChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (ListViewFilter item in e.NewItems)
                {
                    item.PropertyChanged += HandleFilterSelectionChanged;
                }

                // Automatically select first inserted filter
                var firstFilter = filters.FirstOrDefault();
                if (firstFilter != null && !firstFilter.IsSelected)
                {
                    firstFilter.IsSelected = true;
                }
            }

            HasFilters = filters.Any();
        }

        /// <summary>
        /// Handles the field property changed.
        /// </summary>
        private void HandleFieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                ListFieldInfo field = (ListFieldInfo)sender;
                if (field.IsSelected)
                {
                    foreach (var field2 in Fields)
                    {
                        if (field2 != sender)
                        {
                            field2.IsSelected = false;
                        }
                    }

                    OrderBy(field, field.DefaultSortDirection);
                }
                else
                {
                    // TODO: This is getting called even when we set things above, use a boolean guard

                    // Make sure nothing is deselected, override the value if needed
                    if (!Fields.Any(f => f.IsSelected))
                    {
                        field.IsSelected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the filter selection changed.
        /// </summary>
        private void HandleFilterSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected" && ((ListViewFilter)sender).IsSelected)
            {
                foreach (var filter in Filters)
                {
                    if (filter != sender)
                    {
                        filter.IsSelected = false;
                    }
                }

                InvalidateFilters();
            }
        }
    }
}
