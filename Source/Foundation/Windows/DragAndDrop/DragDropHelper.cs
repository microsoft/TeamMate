using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop
{
    /// <summary>
    /// A helper class to initiate a Drag and Drop operation for a given element.
    /// </summary>
    public class DragDropHelper
    {
        private UIElement element;
        private Point initialPosition;
        private MouseButtonEventArgs mouseDownEvent;

        private ListBox listBox;
        private bool isExtendedSelectionListBox;
        private ListBoxItem listBoxItemMouseDown;

        public event EventHandler<MouseButtonEventArgs> DragRequested;

        /// <summary>
        /// Creates an instance for the specified element, registering the appropriate event handlers on the element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The drag and drop helper.</returns>
        public static DragDropHelper Create(UIElement element)
        {
            var helper = new DragDropHelper(element);
            helper.Initialize();
            return helper;
        }

        /// <summary>
        /// Creates a new instance for the given element.
        /// </summary>
        /// <param name="element">The element.</param>
        private DragDropHelper(UIElement element)
        {
            Assert.ParamIsNotNull(element, "element");

            this.element = element;

            this.listBox = this.element as ListBox;
            this.isExtendedSelectionListBox = (listBox != null && listBox.SelectionMode == SelectionMode.Extended);
        }

        /// <summary>
        /// Initializes the event handlers.
        /// </summary>
        public void Initialize()
        {
            this.element.PreviewMouseLeftButtonDown += HandleMouseLeftButtonDown;
            this.element.MouseMove += HandleMouseMove;
            this.element.PreviewMouseLeftButtonUp += HandleMouseLeftButtonUp;
        }

        /// <summary>
        /// Releases the event handlers.
        /// </summary>
        public void Release()
        {
            this.element.PreviewMouseLeftButtonDown -= HandleMouseLeftButtonDown;
            this.element.MouseMove -= HandleMouseMove;
            this.element.PreviewMouseLeftButtonUp -= HandleMouseLeftButtonUp;
        }

        /// <summary>
        /// Handles the mouse left button down.
        /// </summary>
        private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Workaround to deal with this problem http://social.microsoft.com/Forums/fr-FR/e7f213e3-d8c0-4d13-8c39-51b214715c8a/listboxlistview-selection?forum=wpf
            // Extended selection listboxes lose multiple selection when holding the left mouse button down to initiate a drag and drop operation

            mouseDownEvent = e;
            initialPosition = e.GetPosition(element);

            if (isExtendedSelectionListBox && e.ClickCount == 1)
            {
                ListBoxItem item = VisualTreeUtilities.GetItemAt<ListBoxItem>(e);
                if (item != null && item.IsSelected)
                {
                    // KLUDGE: Special case for extended selection list boxes. By default, the behaviour is that you can
                    // use DnD to select more rows or items. We are intercepting that behaviour, and instead, marking the event
                    // as handled. This prevents the listbox from using that event for altering the selection. 

                    if (IsOriginalSourceButton(e))
                    {
                        // Special case. If the innermost item is a button (which likely has a click handler on it), ignore the
                        // event handling and let the default mechanism kick in. Otherwise, we inhibit valid events on a button.
                        // This is a very special case (e.g. clicking on a flag item in a list box item)
                        return;
                    }

                    this.listBoxItemMouseDown = item;
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles the mouse left button up.
        /// </summary>
        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            mouseDownEvent = null;

            if (listBoxItemMouseDown != null)
            {
                ListBoxItem item = VisualTreeUtilities.GetItemAt<ListBoxItem>(e);
                if (item != null && item == listBoxItemMouseDown && this.listBox.SelectedItems.Count > 1)
                {
                    this.listBox.SelectedItems.Clear();
                    item.IsSelected = true;
                    e.Handled = true;
                }

                listBoxItemMouseDown = null;
            }
        }

        /// <summary>
        /// Handles the mouse move.
        /// </summary>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDownEvent != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(element);
                if (DragDropUtilities.ShouldBeginDrag(initialPosition, currentPosition))
                {
                    var eventArgs = mouseDownEvent;
                    mouseDownEvent = null;
                    DragRequested?.Invoke(this, eventArgs);
                }
            }
        }

        /// <summary>
        /// Determines whether the original source of the event was a button.
        /// </summary>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <returns><c>true</c> if the original source was a button.</returns>
        private static bool IsOriginalSourceButton(RoutedEventArgs e)
        {
            bool result = false;

            DependencyObject originalSource = e.OriginalSource as DependencyObject;
            if (originalSource != null)
            {
                var button = VisualTreeUtilities.TryFindAncestorOrSelf<ButtonBase>(originalSource);
                result = (button != null);
            }

            return result;
        }
    }
}
