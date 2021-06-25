using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Interop;
using Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Shell;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Provides utility methods for manipulating windows.
    /// </summary>
    public static class WindowUtilities
    {
        /// <summary>
        /// Moves a window to the standard location for notifications (aligned bottom right, above the task bar).
        /// </summary>
        /// <param name="window">The window to reposition.</param>
        /// <param name="margin">An optional margin to use around the window (default is 20 pixels).</param>
        /// <remarks>
        /// Used by the old style notification (pre Windows 8 toasts).
        /// </remarks>
        public static void MoveToBottomRightNotificationLocation(Window window, int margin = 20)
        {
            Assert.ParamIsNotNull(window, "window");
            Assert.ParamIsNotNegative(margin, "margin");

            double width = (window.ActualWidth > 0) ? window.ActualWidth : window.Width;
            double height = (window.ActualHeight > 0) ? window.ActualHeight : window.Height;
            Rect primaryWorkArea = SystemParameters.WorkArea;
            window.Left = Math.Max(primaryWorkArea.X, primaryWorkArea.X + primaryWorkArea.Width - width - margin);
            window.Top = Math.Max(primaryWorkArea.Y, primaryWorkArea.Y + primaryWorkArea.Height - height - margin);
        }

        public static void MoveToToastLocation(Window window, int instanceCount = 0)
        {
            Assert.ParamIsNotNull(window, "window");
            Assert.ParamIsNotNegative(instanceCount, "instanceCount");

            double width = (window.ActualWidth > 0) ? window.ActualWidth : window.Width;
            double height = (window.ActualHeight > 0) ? window.ActualHeight : window.Height;
            Rect primaryWorkArea = SystemParameters.WorkArea;
            window.Left = Math.Max(primaryWorkArea.X, primaryWorkArea.X + primaryWorkArea.Width - width);
            window.Top = Math.Max(primaryWorkArea.Y, primaryWorkArea.Y + 20 + (instanceCount * (height + 10)));
        }

        public static void ToFullScreen(Window window, Rect? bounds = null)
        {
            window.WindowStyle = WindowStyle.None;
            window.ResizeMode = ResizeMode.NoResize;
            window.WindowState = WindowState.Normal;
            window.Topmost = true;

            if (bounds == null)
            {
                if (window.IsLoaded)
                {
                    bounds = WindowUtilities.GetBoundsForScreenContaining(window);
                }
                else
                {
                    bounds = WindowUtilities.PrimaryScreenBounds;
                }
            }

            window.Left = bounds.Value.Left;
            window.Top = bounds.Value.Top;
            window.Width = bounds.Value.Width;
            window.Height = bounds.Value.Height;
        }

        public static void ToggleFullScreen(Window window)
        {
            FullScreenWindowStateInfo state = UI.GetFullScreenState(window);
            if (state != null)
            {
                state.Restore(window);
                UI.SetFullScreenState(window, null);
            }
            else
            {
                state = new FullScreenWindowStateInfo(window);
                ToFullScreen(window);
                UI.SetFullScreenState(window, state);
            }
        }

        public static void EnsureVisibleAndActive(Window window)
        {
            Assert.ParamIsNotNull(window, "window");

            if (!window.IsVisible)
            {
                window.Show();
            }

            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            window.Activate();
        }

        public static void ForceToForeground(Window window)
        {
            IntPtr hWnd = window.GetHandle();
            ShellUtilities.ForceToForeground(hWnd);
        }

        public static int GetIndexOfScreenContaining(Window window)
        {
            var screen = System.Windows.Forms.Screen.FromHandle(window.GetHandle());
            return TryGetScreenIndex(screen);
        }

        private static int TryGetScreenIndex(System.Windows.Forms.Screen screen)
        {
            int index = -1;

            // E.g. Try to infer screen index from device name...
            string prefix = @"\\.\DISPLAY";

            if (screen.DeviceName.StartsWith(prefix, StringComparison.Ordinal))
            {
                string stringIndex = screen.DeviceName.Substring(prefix.Length);
                if (stringIndex.Length > 0)
                {
                    int parsedIndex;
                    if (Int32.TryParse(stringIndex, out parsedIndex) && parsedIndex >= 1)
                    {
                        index = parsedIndex;
                    }
                }
                else
                {
                    // KLUDGE: Is this a single screen? I haven't confirmed this but I added this for defensiveness
                    Debug.Fail(String.Format("Could not determine screen index from device name: {0}", screen.DeviceName));
                    index = 1;
                }
            }

            if (index >= 0)
            {
                Debug.Assert(index < System.Windows.Forms.SystemInformation.MonitorCount, "Unexpected screen index");
            }

            return index;
        }

        public static Rect GetBoundsForScreenContaining(Window window)
        {
            var physicalWindowBounds = InteropUtilities.LogicalToPhysicalRectangle(window.GetBounds());
            var logicalScreenBounds = InteropUtilities.PhysicalToLogicalRectangle(System.Windows.Forms.Screen.GetBounds(physicalWindowBounds));
            return logicalScreenBounds;
        }

        public static void SizeToPrimaryWorkArea(Window window, double percentage = 0.8)
        {
            Assert.ParamIsNotNull(window, "window");
            Assert.ParamIsWithinRange(percentage, 0.0, 1.0, "percentage");

            Rect primaryWorkArea = SystemParameters.WorkArea;
            window.Width = primaryWorkArea.Width * percentage;
            window.Height = primaryWorkArea.Height * percentage;
        }

        public static void CenterInPrimaryWorkArea(Window window)
        {
            Assert.ParamIsNotNull(window, "window");
            CenterInBounds(window, SystemParameters.WorkArea);
        }

        public static void CenterInBounds(Window window, Rect bounds)
        {
            Assert.ParamIsNotNull(window, "window");

            double width = (window.ActualWidth > 0) ? window.ActualWidth : window.Width;
            double height = (window.ActualHeight > 0) ? window.ActualHeight : window.Height;

            window.Left = Math.Max(bounds.Left, bounds.Left + ((bounds.Width - width) / 2));
            window.Top = Math.Max(bounds.Top, bounds.Top + ((bounds.Height - height) / 2));
        }

        public static void EnsureWithinVirtualScreen(Window window)
        {
            EnsureWithinBounds(window, VirtualScreenBounds);
        }

        public static Rect VirtualScreenBounds
        {
            get
            {
                Rect bounds = new Rect(SystemParameters.VirtualScreenLeft, SystemParameters.VirtualScreenTop,
                                       SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);

                return bounds;
            }
        }

        public static Rect ScreenBoundsFromPoint(Point p)
        {
            var physicalPoint = InteropUtilities.LogicalToPhysicalPoint(p);
            var screen = System.Windows.Forms.Screen.FromPoint(physicalPoint);
            Rect bounds = InteropUtilities.PhysicalToLogicalRectangle(screen.Bounds);
            return bounds;
        }

        public static Rect PrimaryScreenBounds
        {
            get
            {
                Rect bounds = InteropUtilities.PhysicalToLogicalRectangle(System.Windows.Forms.Screen.PrimaryScreen.Bounds);
                return bounds;
            }
        }

        public static Rect SecondaryScreenBounds
        {
            get
            {
                System.Windows.Forms.Screen screen = SecondaryScreen;
                Rect bounds = (screen != null) ? InteropUtilities.PhysicalToLogicalRectangle(screen.Bounds) : new Rect();
                return bounds;
            }
        }

        private static System.Windows.Forms.Screen SecondaryScreen
        {
            get
            {
                System.Windows.Forms.Screen screen = null;
                if (HasMultipleMonitors)
                {
                    var screens = System.Windows.Forms.Screen.AllScreens;
                    if (screens.Length == 2)
                    {
                        // Use a better strategy to figure out monitor 2 in the case of a 2 screen scenario, this works better than 
                        // the line below
                        screen = screens.FirstOrDefault(s => s != System.Windows.Forms.Screen.PrimaryScreen);
                    }
                    else if (screens.Length > 2)
                    {
                        // Try and get this based on the display name, we'll see how it goes
                        screen = screens.FirstOrDefault(s => TryGetScreenIndex(s) == 2);
                    }
                }
                return screen;
            }
        }

        public static void EnsureWithinBounds(Window window, Rect bounds)
        {
            // Make sure we fit within the bounds...
            if (window.Width > bounds.Width)
            {
                window.Width = bounds.Width;
            }

            if (window.Height > bounds.Height)
            {
                window.Height = bounds.Height;
            }

            // Make sure we are not past the left or top of the bounds
            if (window.Left < bounds.Left)
            {
                window.Left = bounds.Left;
            }

            if (window.Top < bounds.Top)
            {
                window.Top = bounds.Top;
            }

            // Make sure we are not past the right or bottom of the bounds
            double horizontalExcess = (window.Left + window.Width) - (bounds.Right);
            double verticalExcess = (window.Top + window.Height) - (bounds.Right);

            if (horizontalExcess > 0)
            {
                window.Left -= horizontalExcess;
            }

            if (verticalExcess > 0)
            {
                window.Top -= verticalExcess;
            }
        }

        public static Point ScreenCursorPosition
        {
            get
            {
                POINT p;

                // IMPORTANT: GetPhysicalCursorPos returns physical coordinates, which need to be converted to Device-Independent Units
                // for setting absolute WPF window coordinates. Otherwise, you will get unexpected results when the DPI is different from 96 (the default).
                // See more at: http://msdn.microsoft.com/en-us/library/ee671605(v=vs.85).aspx and 
                // http://jerryclin.wordpress.com/2007/11/28/dius-not-duis/
                NativeMethods.GetPhysicalCursorPos(out p);

                return InteropUtilities.PhysicalToLogicalPoint(p);
            }
        }


        public static bool HasMultipleMonitors
        {
            get
            {
                return (System.Windows.Forms.SystemInformation.MonitorCount > 1);
            }
        }
    }

    public class FullScreenWindowStateInfo
    {
        public FullScreenWindowStateInfo(Window window)
        {
            WindowState = window.WindowState;
            WindowStyle = window.WindowStyle;
            ResizeMode = window.ResizeMode;
            Left = window.Left;
            Top = window.Top;
            Width = window.Width;
            Height = window.Height;
            Topmost = window.Topmost;
        }

        public void Restore(Window window)
        {
            window.WindowState = WindowState;
            window.WindowStyle = WindowStyle;
            window.ResizeMode = ResizeMode;
            window.Left = Left;
            window.Top = Top;
            window.Width = Width;
            window.Height = Height;
            window.Topmost = Topmost;
        }

        public WindowStyle WindowStyle { get; set; }
        public ResizeMode ResizeMode { get; set; }
        public WindowState WindowState { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Topmost { get; set; }
    }
}
