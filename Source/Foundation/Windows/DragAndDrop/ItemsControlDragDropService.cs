// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop
{
    /// <summary>
    /// A helper class to drag and drop items in an items control to support reordering.
    /// </summary>
    public class ItemsControlDragDropService
    {
        // TODO: Rename to somethign more meaningful

        // source and target
        private DataFormat format = DataFormats.GetDataFormat("DragDropItemsControl");
        private Point initialMousePosition;
        private object draggedData;
        private bool isDragging;
        private DraggedAdorner draggedAdorner;
        private InsertionAdorner insertionAdorner;
        private Window topWindow;

        // source
        private ItemsControl sourceItemsControl;
        private FrameworkElement sourceItemContainer;
        private int sourceIndex;

        // target
        private ItemsControl targetItemsControl;
        private FrameworkElement targetItemContainer;
        private bool hasVerticalOrientation;
        private int insertionIndex;
        private bool isInFirstHalf;

        // singleton
        private static ItemsControlDragDropService instance;

        /// <summary>
        /// Gets a shared instance of the service to keep track of state. Only used privately in this class.
        /// </summary>
        private static ItemsControlDragDropService Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ItemsControlDragDropService();
                }

                return instance;
            }
        }

        /// <summary>
        /// Gets the is drag source property value.
        /// </summary>
        /// <param name="obj">The object.</param>
        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static bool GetIsDragSource(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragSourceProperty);
        }

        /// <summary>
        /// Sets the is drag source property value.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsDragSource(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragSourceProperty, value);
        }

        /// <summary>
        /// The is drag source property
        /// </summary>
        public static readonly DependencyProperty IsDragSourceProperty = DependencyProperty.RegisterAttached(
            "IsDragSource", typeof(bool), typeof(ItemsControlDragDropService), new UIPropertyMetadata(false, OnIsDragSourceChanged));


        /// <summary>
        /// Gets the is drop target property value.
        /// </summary>
        /// <param name="obj">The object.</param>
        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static bool GetIsDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDropTargetProperty);
        }

        /// <summary>
        /// Sets the is drop target property value.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public static void SetIsDropTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDropTargetProperty, value);
        }

        /// <summary>
        /// The is drop target property
        /// </summary>
        public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.RegisterAttached(
            "IsDropTarget", typeof(bool), typeof(ItemsControlDragDropService), new UIPropertyMetadata(false, OnIsDropTargetChanged));

        /// <summary>
        /// Gets the drag drop template.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static DataTemplate GetDragDropTemplate(DependencyObject obj)
        {
            return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
        }

        /// <summary>
        /// Sets the drag drop template.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="value">The value.</param>
        public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
        {
            obj.SetValue(DragDropTemplateProperty, value);
        }

        /// <summary>
        /// The drag drop template property
        /// </summary>
        public static readonly DependencyProperty DragDropTemplateProperty =
            DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(ItemsControlDragDropService), new UIPropertyMetadata(null));

        /// <summary>
        /// Called when is drag source changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnIsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dragSource = obj as ItemsControl;
            if (dragSource != null)
            {
                if (Object.Equals(e.NewValue, true))
                {
                    dragSource.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
                }
                else
                {
                    dragSource.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
                    dragSource.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
                    dragSource.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
                }
            }
        }

        /// <summary>
        /// Called when is drop target changed.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnIsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var dropTarget = obj as ItemsControl;
            if (dropTarget != null)
            {
                if (Object.Equals(e.NewValue, true))
                {
                    dropTarget.AllowDrop = true;
                    dropTarget.PreviewDrop += Instance.DropTarget_PreviewDrop;
                    dropTarget.PreviewDragEnter += Instance.DropTarget_PreviewDragEnter;
                    dropTarget.PreviewDragOver += Instance.DropTarget_PreviewDragOver;
                    dropTarget.PreviewDragLeave += Instance.DropTarget_PreviewDragLeave;
                }
                else
                {
                    dropTarget.AllowDrop = false;
                    dropTarget.PreviewDrop -= Instance.DropTarget_PreviewDrop;
                    dropTarget.PreviewDragEnter -= Instance.DropTarget_PreviewDragEnter;
                    dropTarget.PreviewDragOver -= Instance.DropTarget_PreviewDragOver;
                    dropTarget.PreviewDragLeave -= Instance.DropTarget_PreviewDragLeave;
                }
            }
        }

        // DragSource

        /// <summary>
        /// Handles the PreviewMouseLeftButtonDown event of the DragSource control.
        /// </summary>
        private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                DependencyObject visual = e.OriginalSource as DependencyObject;

                ItemsControl itemsControl = (ItemsControl)sender;
                FrameworkElement itemContainer = itemsControl.ContainerFromElement(visual) as FrameworkElement;
                if (itemContainer != null)
                {
                    this.sourceItemsControl = itemsControl;
                    this.sourceItemContainer = itemContainer;

                    this.topWindow = Window.GetWindow(this.sourceItemsControl);
                    this.initialMousePosition = e.GetPosition(this.topWindow);
                    this.sourceIndex = this.sourceItemsControl.ItemContainerGenerator.IndexFromContainer(this.sourceItemContainer);
                    this.draggedData = this.sourceItemContainer.DataContext;
                }
            }
        }

        /// <summary>
        /// Handles the PreviewMouseMove event of the DragSource control.
        /// </summary>
        private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (this.draggedData != null)
            {
                // Only drag when user moved the mouse by a reasonable amount.
                if (DragDropUtilities.ShouldBeginDrag(this.initialMousePosition, e.GetPosition(this.topWindow)))
                {
                    DataObject data = new DataObject(this.format.Name, this.draggedData);

                    // Adding events to the window to make sure dragged adorner comes up when mouse is not over a drop target.
                    Window window = this.topWindow;
                    bool previousAllowDrop = window.AllowDrop;
                    window.AllowDrop = true;
                    window.DragEnter += TopWindow_DragEnter;
                    window.DragOver += TopWindow_DragOver;
                    window.DragLeave += TopWindow_DragLeave;

                    this.isDragging = true;
                    DragDropEffects effects = DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);

                    // Without this call, there would be a bug in the following scenario: Click on a data item, and drag
                    // the mouse very fast outside of the window. When doing this really fast, for some reason I don't get 
                    // the Window leave event, and the dragged adorner is left behind.
                    // With this call, the dragged adorner will disappear when we release the mouse outside of the window,
                    // which is when the DoDragDrop synchronous method returns.
                    RemoveDraggedAdorner();

                    window.AllowDrop = previousAllowDrop;
                    window.DragEnter -= TopWindow_DragEnter;
                    window.DragOver -= TopWindow_DragOver;
                    window.DragLeave -= TopWindow_DragLeave;

                    this.draggedData = null;
                }
            }
        }

        /// <summary>
        /// Handles the PreviewMouseLeftButtonUp event of the DragSource control.
        /// </summary>
        private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // If i was dragging, don't let other mouse up handlers kick in...
            e.Handled = this.isDragging;
            Reset();
        }

        /// <summary>
        /// Resets the drag and drop tracking state.
        /// </summary>
        private void Reset()
        {
            this.isDragging = false;
            this.draggedData = null;
            this.sourceIndex = -1;
            this.topWindow = null;

            this.sourceItemsControl = null;
            this.sourceItemContainer = null;
            this.sourceIndex = -1;

            this.targetItemsControl = null;
            this.targetItemsControl = null;
            this.hasVerticalOrientation = false;
            this.insertionIndex = -1;
            this.isInFirstHalf = false;
        }

        // DropTarget

        /// <summary>
        /// Handles the PreviewDragEnter event of the DropTarget control.
        /// </summary>
        private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
        {
            this.targetItemsControl = (ItemsControl)sender;
            object draggedItem = e.Data.GetData(this.format.Name);

            DecideDropTarget(e);
            if (draggedItem != null)
            {
                // Dragged Adorner is created on the first enter only.
                ShowDraggedAdorner(e.GetPosition(this.topWindow));
                CreateInsertionAdorner();
            }
            e.Handled = true;
        }

        /// <summary>
        /// Handles the PreviewDragOver event of the DropTarget control.
        /// </summary>
        private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
        {
            object draggedItem = e.Data.GetData(this.format.Name);

            DecideDropTarget(e);
            if (draggedItem != null)
            {
                Point currentPosition = e.GetPosition(this.topWindow);

                // Dragged Adorner is only updated here - it has already been created in DragEnter.
                ShowDraggedAdorner(currentPosition);
                UpdateInsertionAdornerPosition();

                FrameworkElement element = e.OriginalSource as FrameworkElement;
                if (element != null)
                {
                    var scrolled = DragDropUtilities.AutoScrollIfNeeded(element, e.GetPosition(element));
                    if (scrolled.X != 0 || scrolled.Y != 0)
                    {
                        // TODO: This is not working still, not doing what I need it to be doing...
                        this.draggedAdorner.SetPosition(currentPosition.X - this.initialMousePosition.X + scrolled.X, currentPosition.Y - this.initialMousePosition.Y + scrolled.Y);
                    }
                }
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles the PreviewDrop event of the DropTarget control.
        /// </summary>
        private void DropTarget_PreviewDrop(object sender, DragEventArgs e)
        {
            object draggedItem = e.Data.GetData(this.format.Name);

            if (draggedItem != null && sourceIndex >= 0)
            {
                bool alreadyMoved = false;

                if ((e.Effects & DragDropEffects.Move) != 0)
                {
                    if (this.sourceItemsControl == this.targetItemsControl)
                    {
                        if (insertionIndex == sourceIndex || insertionIndex == sourceIndex + 1)
                        {
                            // Dropping on the same old position, nothing to do here...
                            alreadyMoved = true;
                        }
                        else if (MoveItem(this.sourceItemsControl, sourceIndex, insertionIndex))
                        {
                            alreadyMoved = true;
                        }
                    }

                    if (!alreadyMoved)
                    {
                        RemoveItem(this.sourceItemsControl, sourceIndex);
                    }
                }

                if (!alreadyMoved)
                {
                    // This happens when we drag an item to a later position within the same ItemsControl.
                    if (this.sourceItemsControl == this.targetItemsControl && this.sourceIndex < this.insertionIndex)
                    {
                        this.insertionIndex--;
                    }

                    InsertItem(this.targetItemsControl, this.insertionIndex, draggedItem);

                }

                RemoveDraggedAdorner();
                RemoveInsertionAdorner();
                Reset();
            }

            e.Handled = true;
        }

        /// <summary>
        /// Handles the PreviewDragLeave event of the DropTarget control.
        /// </summary>
        private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
        {
            // Dragged Adorner is only created once on DragEnter + every time we enter the window. 
            // It's only removed once on the DragDrop, and every time we leave the window. (so no need to remove it here)
            object draggedItem = e.Data.GetData(this.format.Name);

            if (draggedItem != null)
            {
                RemoveInsertionAdorner();
            }
            e.Handled = true;
        }

        /// <summary>
        /// Decides the drop target and target attributes for a given drag event.
        /// </summary>
        /// <param name="e">The <see cref="DragEventArgs"/> instance containing the event data.</param>
        private void DecideDropTarget(DragEventArgs e)
        {
            // If the types of the dragged data and ItemsControl's source are compatible, 
            // there are 3 situations to have into account when deciding the drop target:
            // 1. mouse is over an items container
            // 2. mouse is over the empty part of an ItemsControl, but ItemsControl is not empty
            // 3. mouse is over an empty ItemsControl.

            // The goal of this method is to decide on the values of the following properties: 
            // targetItemContainer, insertionIndex and isInFirstHalf.

            int targetItemsControlCount = this.targetItemsControl.Items.Count;
            object draggedItem = e.Data.GetData(this.format.Name);

            if (IsDropDataTypeAllowed(draggedItem))
            {
                if (targetItemsControlCount > 0)
                {
                    this.hasVerticalOrientation = HasVerticalOrientation(this.targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
                    this.targetItemContainer = this.targetItemsControl.ContainerFromElement(e.OriginalSource as DependencyObject) as FrameworkElement;

                    if (this.targetItemContainer != null)
                    {
                        Point positionRelativeToItemContainer = e.GetPosition(this.targetItemContainer);
                        this.isInFirstHalf = IsInFirstHalf(this.targetItemContainer, positionRelativeToItemContainer, this.hasVerticalOrientation);
                        this.insertionIndex = this.targetItemsControl.ItemContainerGenerator.IndexFromContainer(this.targetItemContainer);

                        if (!this.isInFirstHalf)
                        {
                            this.insertionIndex++;
                        }
                    }
                    else
                    {
                        this.targetItemContainer = this.targetItemsControl.ItemContainerGenerator.ContainerFromIndex(targetItemsControlCount - 1) as FrameworkElement;
                        this.isInFirstHalf = false;
                        this.insertionIndex = targetItemsControlCount;
                    }
                }
                else
                {
                    this.targetItemContainer = null;
                    this.insertionIndex = 0;
                }
            }
            else
            {
                this.targetItemContainer = null;
                this.insertionIndex = -1;
                e.Effects = DragDropEffects.None;
            }
        }

        /// <summary>
        /// Determines whether the dragged item can be dropped in a destination items control.
        /// </summary>
        /// <param name="draggedItem">The dragged item.</param>
        /// <returns><c>true</c> if the target control can accept the drop, otherwise false.</returns>
        private bool IsDropDataTypeAllowed(object draggedItem)
        {
            bool isDropDataTypeAllowed;
            IEnumerable collectionSource = this.targetItemsControl.ItemsSource;
            if (draggedItem != null)
            {
                if (collectionSource != null)
                {
                    Type draggedType = draggedItem.GetType();
                    Type collectionType = collectionSource.GetType();

                    Type genericIListType = collectionType.GetInterface("IList`1");
                    if (genericIListType != null)
                    {
                        Type[] genericArguments = genericIListType.GetGenericArguments();
                        isDropDataTypeAllowed = genericArguments[0].IsAssignableFrom(draggedType);
                    }
                    else if (typeof(IList).IsAssignableFrom(collectionType))
                    {
                        isDropDataTypeAllowed = true;
                    }
                    else
                    {
                        isDropDataTypeAllowed = false;
                    }
                }
                else // the ItemsControl's ItemsSource is not data bound.
                {
                    isDropDataTypeAllowed = true;
                }
            }
            else
            {
                isDropDataTypeAllowed = false;
            }
            return isDropDataTypeAllowed;
        }

        // Window

        /// <summary>
        /// Handles the DragEnter event of the window control.
        /// </summary>
        private void TopWindow_DragEnter(object sender, DragEventArgs e)
        {
            ShowDraggedAdorner(e.GetPosition(this.topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// Handles the DragOver event of the window control.
        /// </summary>
        private void TopWindow_DragOver(object sender, DragEventArgs e)
        {
            ShowDraggedAdorner(e.GetPosition(this.topWindow));
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        /// <summary>
        /// Handles the DragLeave event of the window control.
        /// </summary>
        private void TopWindow_DragLeave(object sender, DragEventArgs e)
        {
            RemoveDraggedAdorner();
            e.Handled = true;
        }

        // Adorners

        /// <summary>
        /// Creates or updates the dragged Adorner. 
        /// </summary>
        /// <param name="currentPosition">The current position.</param>
        private void ShowDraggedAdorner(Point currentPosition)
        {
            if (this.draggedAdorner == null)
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(this.sourceItemsControl);
                this.draggedAdorner = new DraggedAdorner(this.sourceItemContainer, adornerLayer);
            }
            this.draggedAdorner.SetPosition(currentPosition.X - this.initialMousePosition.X, currentPosition.Y - this.initialMousePosition.Y);
        }

        /// <summary>
        /// Removes the dragged adorner.
        /// </summary>
        private void RemoveDraggedAdorner()
        {
            if (this.draggedAdorner != null)
            {
                this.draggedAdorner.Detach();
                this.draggedAdorner = null;
            }
        }

        /// <summary>
        /// Creates the insertion adorner.
        /// </summary>
        private void CreateInsertionAdorner()
        {
            if (this.targetItemContainer != null)
            {
                // Here, I need to get adorner layer from targetItemContainer and not targetItemsControl. 
                // This way I get the AdornerLayer within ScrollContentPresenter, and not the one under AdornerDecorator (Snoop is awesome).
                // If I used targetItemsControl, the adorner would hang out of ItemsControl when there's a horizontal scroll bar.
                var adornerLayer = AdornerLayer.GetAdornerLayer(this.targetItemContainer);
                this.insertionAdorner = new InsertionAdorner(this.hasVerticalOrientation, this.isInFirstHalf, this.targetItemContainer, adornerLayer);
            }
        }

        /// <summary>
        /// Updates the insertion adorner position.
        /// </summary>
        private void UpdateInsertionAdornerPosition()
        {
            if (this.insertionAdorner != null)
            {
                this.insertionAdorner.IsInFirstHalf = this.isInFirstHalf;
                this.insertionAdorner.InvalidateVisual();
            }
        }

        /// <summary>
        /// Removes the insertion adorner.
        /// </summary>
        private void RemoveInsertionAdorner()
        {
            if (this.insertionAdorner != null)
            {
                this.insertionAdorner.Detach();
                this.insertionAdorner = null;
            }
        }

        /// <summary>
        /// Moves the item within an items control from an old index to the new index.
        /// </summary>
        /// <param name="itemsControl">The items control.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">Index of the insertion.</param>
        /// <returns><c>true</c> if the item was successfully moved.</returns>
        private static bool MoveItem(ItemsControl itemsControl, int oldIndex, int newIndex)
        {
            bool moved = false;

            if (newIndex != oldIndex && newIndex != oldIndex + 1)
            {
                if (oldIndex < newIndex)
                {
                    // If moving after myself, need to adjust the target index;
                    newIndex--;
                }

                IEnumerable itemsSource = itemsControl.ItemsSource;
                if (itemsSource != null)
                {
                    Type type = itemsSource.GetType();
                    if (IsObservableCollectionType(type))
                    {
                        type.GetMethod("Move").Invoke(itemsSource, new object[] { oldIndex, newIndex });
                        moved = true;
                    }
                }
            }

            return moved;
        }

        /// <summary>
        /// Inserts the item in a target items control.
        /// </summary>
        /// <param name="itemsControl">The items control.</param>
        /// <param name="index">The insertion index.</param>
        /// <param name="item">The item to insert.</param>
        private static void InsertItem(ItemsControl itemsControl, int index, object item)
        {
            bool inserted = false;
            IEnumerable itemsSource = itemsControl.ItemsSource;

            // Is the ItemsSource IList or IList<T>? If so, insert the dragged item in the list.
            if (itemsSource is IList)
            {
                ((IList)itemsSource).Insert(index, item);
                inserted = true;
            }
            else
            {
                Type type = itemsSource.GetType();
                if (IsGenericListType(type))
                {
                    type.GetMethod("Insert").Invoke(itemsSource, new object[] { index, item });
                    inserted = true;
                }
            }

            if (!inserted)
            {
                // Fallback to inserting directly in itemsControl
                itemsControl.Items.Insert(index, item);
            }
        }

        /// <summary>
        /// Removes the item from an items control.
        /// </summary>
        /// <param name="itemsControl">The items control.</param>
        /// <param name="index">The index.</param>
        private static void RemoveItem(ItemsControl itemsControl, int index)
        {
            bool removed = false;
            IEnumerable itemsSource = itemsControl.ItemsSource;

            // Is the ItemsSource IList or IList<T>? If so, remove the item from the list.
            if (itemsSource is IList)
            {
                ((IList)itemsSource).RemoveAt(index);
                removed = true;
            }
            else
            {
                Type type = itemsSource.GetType();
                Type genericIListType = type.GetInterface("IList`1");
                if (genericIListType != null)
                {
                    type.GetMethod("RemoveAt").Invoke(itemsSource, new object[] { index });
                    removed = true;
                }
            }

            if (!removed)
            {
                itemsControl.Items.RemoveAt(index);
            }
        }

        /// <summary>
        /// Determines whether a given point is in the first (or second) half of the specified container. This helps
        /// determine the insertion index and painting of the adorner in the right location.
        /// </summary>
        /// <param name="container">The container.</param>
        /// <param name="clickedPoint">The clicked point.</param>
        /// <param name="hasVerticalOrientation">if set to <c>true</c> the container has a vertical orientation.</param>
        /// <returns><c>true</c> if the point corresponds to the first half of the container, otherwise <c>false</c></returns>
        private static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation)
        {
            if (hasVerticalOrientation)
            {
                return clickedPoint.Y < container.ActualHeight / 2;
            }

            return clickedPoint.X < container.ActualWidth / 2;
        }

        /// <summary>
        /// Finds the orientation of the panel of the ItemsControl that contains the itemContainer passed as a parameter.
        /// The orientation is needed to figure out where to draw the adorner that indicates where the item will be dropped.
        /// </summary>
        /// <param name="itemContainer">The item container.</param>
        /// <returns><c>true</c> if the container has a vertical orientation.</returns>
        private static bool HasVerticalOrientation(FrameworkElement itemContainer)
        {
            bool hasVerticalOrientation = true;
            if (itemContainer != null)
            {
                Panel panel = VisualTreeHelper.GetParent(itemContainer) as Panel;
                StackPanel stackPanel;
                WrapPanel wrapPanel;

                if ((stackPanel = panel as StackPanel) != null)
                {
                    hasVerticalOrientation = (stackPanel.Orientation == Orientation.Vertical);
                }
                else if ((wrapPanel = panel as WrapPanel) != null)
                {
                    hasVerticalOrientation = (wrapPanel.Orientation == Orientation.Vertical);
                }
                // You can add support for more panel types here.
            }

            return hasVerticalOrientation;
        }

        /// <summary>
        /// Determines whether an items is of a generic ObservableCollection type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the type is an ObservableCollection type</returns>
        private static bool IsObservableCollectionType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ObservableCollection<>);
        }

        /// <summary>
        /// Determines whether an items is of a generic IList type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><c>true</c> if the type is an IList type</returns>
        private static bool IsGenericListType(Type type)
        {
            return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IList<>));
        }

    }
}
