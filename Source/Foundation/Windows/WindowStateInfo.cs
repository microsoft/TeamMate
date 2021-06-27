// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public class WindowStateInfo
    {
        public Rect RestoreBounds { get; private set; }
        public WindowState WindowState { get; private set; }

        public WindowStateInfo(WindowState state, Rect bounds)
        {
            this.WindowState = state;
            this.RestoreBounds = bounds;
        }

        public static WindowStateInfo Capture(Window window)
        {
            return new WindowStateInfo(window.WindowState, window.RestoreBounds);
        }

        public void Apply(Window window)
        {
            if (!RestoreBounds.IsEmpty)
            {
                window.Left = RestoreBounds.Left;
                window.Top = RestoreBounds.Top;
                window.Width = RestoreBounds.Width;
                window.Height = RestoreBounds.Height;
            }

            // If we are restoring old bounds, make sure we are still within the visible virtual screen
            // (e.g. what if bounds are no longer valid, or monitor configuration has changed)?
            WindowUtilities.EnsureWithinVirtualScreen(window);

            if (window.IsLoaded && WindowState != WindowState.Minimized)
            {
                window.WindowState = WindowState;
            }
        }

        public void ApplyLocationOnly(Window window)
        {
            if (!RestoreBounds.IsEmpty)
            {
                window.Left = RestoreBounds.Left;
                window.Top = RestoreBounds.Top;
            }

            // If we are restoring old bounds, make sure we are still within the visible virtual screen
            // (e.g. what if bounds are no longer valid, or monitor configuration has changed)?
            WindowUtilities.EnsureWithinVirtualScreen(window);
        }

        public override int GetHashCode()
        {
            return WindowState.GetHashCode() + RestoreBounds.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            WindowStateInfo other = obj as WindowStateInfo;
            if (other != null)
            {
                return RestoreBounds.Equals(other.RestoreBounds) && WindowState == other.WindowState;
            }

            return base.Equals(obj);
        }
    }
}
