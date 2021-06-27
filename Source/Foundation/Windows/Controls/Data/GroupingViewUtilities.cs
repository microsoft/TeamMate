using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// Provides utility methods for managing grouped views (e.g. list view groups).
    /// </summary>
    public static class GroupingViewUtilities
    {
        /// <summary>
        /// The IsExpanded dependency property.
        /// </summary>
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.RegisterAttached(
            "IsExpanded", typeof(bool), typeof(GroupingViewUtilities), new PropertyMetadata(true, OnIsExpandedChanged)
        );

        /// <summary>
        /// Sets the IsExpanded dependency property.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="value">A value determining whether the element is expanded or not.</param>
        public static void SetIsExpanded(DependencyObject element, bool value)
        {
            element.SetValue(IsExpandedProperty, value);
        }

        /// <summary>
        /// Gets the value of the IsExpanded dependency property.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns><c>true</c> if expanded, otherwise <c>false</c>.</returns>
        [AttachedPropertyBrowsableForType(typeof(GroupItem))]
        public static bool GetIsExpanded(DependencyObject element)
        {
            return (bool)element.GetValue(IsExpandedProperty);
        }

        /// <summary>
        /// Handles a change in the value of the IsExpanded property.
        /// </summary>
        private static void OnIsExpandedChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
        {
            GroupItem groupItem = element as GroupItem;

            if (groupItem != null)
            {
                bool isExpanded = (bool)e.NewValue;
                if (!isExpanded)
                {
                    // A group item was collapsed. 

                    // If it contained keyboard focus, focus the first focusable element, which should be the header.
                    bool keyboardFocusWithin = groupItem.IsKeyboardFocusWithin;
                    if (keyboardFocusWithin)
                    {
                        var firstFocusable = VisualTreeUtilities.Descendants(groupItem).OfType<FrameworkElement>().FirstOrDefault(fe => fe.Focusable);
                        if (firstFocusable != null)
                        {
                            firstFocusable.Focus();
                        }
                    }

                    // Also, deselect any of its children that were previously selected.
                    var listBoxItems = VisualTreeUtilities.FirstDescendantsOfType<ListBoxItem>(groupItem);
                    foreach (var listBoxItem in listBoxItems)
                    {
                        if (listBoxItem.IsSelected)
                        {
                            listBoxItem.IsSelected = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines whether the specified list box is in grouping mode.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        /// <returns><c>true</c> if the list box is currently in grouping mode.</returns>
        public static bool IsGrouping(ListBox listBox)
        {
            Assert.ParamIsNotNull(listBox, "listBox");

            ICollectionView collectionView = listBox.ItemsSource as ICollectionView;
            return (collectionView != null && collectionView.Groups != null && collectionView.Groups.Any());
        }

        /// <summary>
        /// Expands or collapses all groups in a list box.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        /// <param name="isExpanded">if set to <c>true</c>, expand all groups. Otherwise, collapse them..</param>
        public static void SetIsExpandedOnAllGroups(ListBox listBox, bool isExpanded)
        {
            Assert.ParamIsNotNull(listBox, "listBox");

            var groups = GetGroups(listBox);
            foreach (var group in groups)
            {
                SetIsExpanded(group, isExpanded);
            }
        }

        /// <summary>
        /// Gets the group items for a given list box.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        /// <returns>The group items, or an emtpy array if none are available.</returns>
        public static GroupItem[] GetGroups(ListBox listBox)
        {
            Assert.ParamIsNotNull(listBox, "listBox");

            if (!IsGrouping(listBox))
            {
                return new GroupItem[0];
            }

            return VisualTreeUtilities.FirstDescendantsOfType<GroupItem>(listBox).ToArray();
        }

        /// <summary>
        /// Tries to get the group item that contains the given element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The ancestor group item, or <c>null</c> if the element was not a descendant of a group item.</returns>
        public static GroupItem GetContainerGroupItem(FrameworkElement element)
        {
            Assert.ParamIsNotNull(element, "element");

            // TODO: Stop ancestor search at list box level
            GroupItem groupItem = VisualTreeUtilities.TryFindAncestor<GroupItem>(element);
            return groupItem;
        }

        /// <summary>
        /// Toggles the expanded state of a group.
        /// </summary>
        /// <param name="groupItem">The group item.</param>
        public static void ToggleIsExpanded(GroupItem groupItem)
        {
            Assert.ParamIsNotNull(groupItem, "groupItem");

            SetIsExpanded(groupItem, !GetIsExpanded(groupItem));
        }

        /// <summary>
        /// Captures the selection and group expanded state of a list box, and returns a disposable
        /// object that will restore the state. This allow for list box modifications that would otherwise
        /// lose the state.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        /// <returns>A disposable object that will restore the original state of the list box when disposed.</returns>
        public static IDisposable CaptureGroupingState(ListBox listBox)
        {
            Assert.ParamIsNotNull(listBox, "listBox");

            return new ListBoxGroupMemento(listBox);
        }

        /// <summary>
        /// Registers the default keyboard and mouse shortcuts on a given list box for group expansion and collapsing.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        public static void RegisterDefaultShortcuts(ListBox listBox)
        {
            Assert.ParamIsNotNull(listBox, "listBox");

            listBox.KeyDown += HandleListBoxKeyDown;
            listBox.PreviewMouseDown += HandleListBoxPreviewMouseDown;
        }

        /// <summary>
        /// Unregisters the default keyboard and mouse shortcuts on a given list box..
        /// </summary>
        /// <param name="listBox">The list box.</param>
        public static void UnregisterDefaultShortcuts(ListBox listBox)
        {
            Assert.ParamIsNotNull(listBox, "listBox");

            listBox.KeyDown -= HandleListBoxKeyDown;
            listBox.PreviewMouseDown -= HandleListBoxPreviewMouseDown;
        }

        /// <summary>
        /// Handles a key press in a list box.
        /// </summary>
        private static void HandleListBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                // If on a group item, expand/collapse it
                // if in a group container, 
                var element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    bool expand = (e.Key == Key.Right);
                    bool all = (Keyboard.Modifiers == ModifierKeys.Shift);

                    if (!all)
                    {
                        GroupItem groupItem = GetContainerGroupItem(element);
                        if (groupItem != null)
                        {
                            SetIsExpanded(groupItem, expand);
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        SetIsExpandedOnAllGroups((ListBox)sender, expand);
                        e.Handled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Handles a mouse down event in a list box.
        /// </summary>
        private static void HandleListBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                var element = e.OriginalSource as FrameworkElement;
                var group = (element != null) ? element.DataContext as CollectionViewGroup : null;
                if (group != null)
                {
                    GroupItem groupItem = GetContainerGroupItem(element);
                    if (groupItem != null)
                    {
                        ToggleIsExpanded(groupItem);
                        e.Handled = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// A helper memento class to capture and restore the state of a listbox.
    /// </summary>
    internal class ListBoxGroupMemento : IDisposable
    {
        private ListBox listBox;
        private object selectedItem;
        private IDictionary<object, bool> expandedStates = new Dictionary<object, bool>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ListBoxGroupMemento"/> class.
        /// </summary>
        /// <param name="listBox">The list box.</param>
        public ListBoxGroupMemento(ListBox listBox)
        {
            this.listBox = listBox;
            Capture();
        }

        /// <summary>
        /// Captures the state of the listbox.
        /// </summary>
        public void Capture()
        {
            this.selectedItem = listBox.SelectedItem;
            expandedStates.Clear();

            foreach (var group in GroupingViewUtilities.GetGroups(listBox))
            {
                expandedStates[((CollectionViewGroup)group.DataContext).Name] = GroupingViewUtilities.GetIsExpanded(group);
            }
        }

        /// <summary>
        /// Restores the state of the list box.
        /// </summary>
        public void Restore()
        {
            listBox.UpdateLayout();
            listBox.SelectedItem = this.selectedItem;

            if (this.selectedItem != null)
            {
                var container = listBox.ItemContainerGenerator.ContainerFromItem(this.selectedItem) as FrameworkElement;
                if (container != null)
                {
                    container.BringIntoView();
                }
            }

            foreach (var group in GroupingViewUtilities.GetGroups(listBox))
            {
                bool expanded;
                if (expandedStates.TryGetValue(((CollectionViewGroup)group.DataContext).Name, out expanded))
                {
                    GroupingViewUtilities.SetIsExpanded(group, expanded);
                }
            }
        }

        public void Dispose()
        {
            Restore();
        }
    }
}
