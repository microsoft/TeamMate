using Microsoft.Tools.TeamMate.Foundation.Windows.Media;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.DragAndDrop
{
    /// <summary>
    /// Provides Drag and Drop Utility Methods.
    /// </summary>
    public static class DragDropUtilities
    {
        private const int AutoScrollTolerance = 10;

        /// <summary>
        /// Determines if the distance between two points is big enough to indicate
        /// that a drag operation has started.
        /// </summary>
        /// <param name="initialPosition">The initial position.</param>
        /// <param name="currentPosition">The current position.</param>
        /// <returns><c>true</c> if the distance indicates that a drag operation is in progress.</returns>
        public static bool ShouldBeginDrag(Point initialPosition, Point currentPosition)
        {
            return (Math.Abs(currentPosition.X - initialPosition.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                 Math.Abs(currentPosition.Y - initialPosition.Y) >= SystemParameters.MinimumVerticalDragDistance);
        }

        /// <summary>
        /// Automatically scrolls a UI element if needed during drag.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="point">The point.</param>
        /// <param name="autoScrollOffset">The automatic scroll offset.</param>
        /// <returns>Returns a point with the values indicating the horizontal and vertical scroll distances.</returns>
        public static Point AutoScrollIfNeeded(UIElement element, Point point, int autoScrollOffset = 10)
        {
            // BUG: AutoScroll is not repositioning elements in the adorner layer when it is moving, maybe it needs to return a value so that we can reposition? (e.g. x and y change?)
            double verticalScroll = 0;
            double horizontalScroll = 0;

            ScrollViewer scrollViewer = VisualTreeUtilities.TryFindAncestor<ScrollViewer>(element);
            if (scrollViewer != null)
            {
                Point scrollViewerPont = element.TranslatePoint(point, scrollViewer);
                double hoff = scrollViewer.HorizontalOffset;
                double voff = scrollViewer.VerticalOffset;

                if (scrollViewer.ScrollableHeight > 0)
                {
                    if (scrollViewerPont.Y < AutoScrollTolerance) // Top of visible list? 
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - autoScrollOffset); //Scroll up. 
                    }
                    else if (scrollViewerPont.Y > scrollViewer.ActualHeight - AutoScrollTolerance) //Bottom of visible list? 
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + autoScrollOffset); //Scroll down.     
                    }
                }

                if (scrollViewer.ScrollableWidth > 0)
                {
                    if (scrollViewerPont.X < AutoScrollTolerance) // Left of visible list? 
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - autoScrollOffset); //Scroll left. 
                    }
                    else if (scrollViewerPont.X > scrollViewer.ActualWidth - AutoScrollTolerance) //Right of visible list? 
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + autoScrollOffset); //Scroll right.     
                    }
                }

                horizontalScroll = scrollViewer.HorizontalOffset - hoff;
                verticalScroll = scrollViewer.VerticalOffset - voff;
            }

            return new Point(horizontalScroll, verticalScroll);
        }
    }
}
