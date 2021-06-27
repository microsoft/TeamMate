// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop
{
    /// <summary>
    /// A helper class to support moving windows by clicking and dragging on a portion of the window,
    /// within a given set of bounds.
    /// </summary>
    public class DragMoveWithinBoundsHelper
    {
        private Rect dragBounds = WindowUtilities.VirtualScreenBounds;

        private Window window;
        private bool isMouseDown;

        private Point lastPoint;
        private Point lastWindowPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="DragMoveWithinBoundsHelper"/> class.
        /// </summary>
        /// <param name="window">The window.</param>
        public DragMoveWithinBoundsHelper(Window window)
        {
            this.window = window;
        }

        /// <summary>
        /// Begins the DragMove operation.
        /// </summary>
        public void DragMove()
        {
            isMouseDown = true;
            lastPoint = WindowUtilities.ScreenCursorPosition;
            lastWindowPoint = new Point(window.Left, window.Top);

            window.PreviewMouseMove += HandleMouseMove;
            window.PreviewMouseLeftButtonUp += HandleMouseLeftButtonUp;
            window.CaptureMouse();
        }

        /// <summary>
        /// Handles the mouse left button up.
        /// </summary>
        private void HandleMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
            window.ReleaseMouseCapture();

            window.PreviewMouseMove -= HandleMouseMove;
            window.PreviewMouseLeftButtonUp -= HandleMouseLeftButtonUp;
        }

        /// <summary>
        /// Handles the mouse move.
        /// </summary>
        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (isMouseDown)
            {
                Point newPoint = WindowUtilities.ScreenCursorPosition;

                double left = newPoint.X - lastPoint.X + lastWindowPoint.X;
                double top = newPoint.Y - lastPoint.Y + lastWindowPoint.Y;

                // Make sure the window doesn't go out of bounds
                window.Left = Math.Max(0, Math.Min(dragBounds.Width - window.Width, left));
                window.Top = Math.Max(0, Math.Min(dragBounds.Height - window.Height, top));

                lastPoint = newPoint;
                lastWindowPoint = new Point(window.Left, window.Top);
            }
        }
    }
}
