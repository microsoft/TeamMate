// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Native;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Shell
{
    public class WindowInfo
    {
        private IntPtr hWnd;
        private WINDOWINFO nativeInfo;
        private int processId;
        private string windowText;
        private IntPtr parent;

        public static WindowInfo FromHwnd(IntPtr hWnd)
        {
            WindowInfo info = new WindowInfo(hWnd);
            info.Refresh();
            return info;
        }

        public static ICollection<WindowInfo> GetVisibleTopLevelWindows()
        {
            return GetDesktopWindows(IsVisibleAndTopLevel);
        }

        public static ICollection<WindowInfo> GetVisibleDesktopWindows()
        {
            return GetDesktopWindows(IsVisibleWindow);
        }

        public static ICollection<WindowInfo> GetDesktopWindows(Predicate<IntPtr> predicate)
        {
            List<WindowInfo> windows = new List<WindowInfo>();

            uint threadId = NativeMethods.GetCurrentThreadId();
            IntPtr desktop = NativeMethods.GetThreadDesktop(threadId);

            NativeMethods.EnumDesktopWindows(desktop, delegate(IntPtr hWnd, IntPtr lParam)
            {
                if (predicate(hWnd))
                {
                    WindowInfo info = WindowInfo.FromHwnd(hWnd);
                    if (info.IsApplicationWindow)
                        windows.Add(info);
                }

                return true;
            }, IntPtr.Zero);

            return windows;
        }

        private static bool IsVisibleAndTopLevel(IntPtr hWnd)
        {
            return IsVisibleWindow(hWnd) && NativeMethods.GetParent(hWnd) == IntPtr.Zero;
        }

        private static bool IsVisibleWindow(IntPtr hWnd)
        {
            return NativeMethods.IsWindow(hWnd) && NativeMethods.IsWindowVisible(hWnd);
        }

        private WindowInfo(IntPtr hWnd)
        {
            this.hWnd = hWnd;
        }

        public void Refresh()
        {
            uint processId;
            NativeMethods.GetWindowThreadProcessId(hWnd, out processId);

            string windowText = NativeMethods.GetWindowText(hWnd);

            WINDOWINFO info = WINDOWINFO.Create();
            NativeMethods.GetWindowInfo(hWnd, ref info);

            IntPtr parentHwnd = NativeMethods.GetParent(hWnd);

            this.parent = parentHwnd;
            this.nativeInfo = info;
            this.processId = (int)processId;
            this.windowText = windowText;
        }

        public Int32Rect VisibleWindowBounds
        {
            get
            {
                var rect = WindowBounds.ToRectangle();
                rect.Intersect(SystemInformation.VirtualScreen);
                return rect.ToInt32Rect();
            }
        }

        public Int32Rect WindowBounds
        {
            get { return this.nativeInfo.rcWindow; }
        }

        public Int32Rect ClientBounds
        {
            get { return this.nativeInfo.rcClient; }
        }

        public int WindowBorderWidth
        {
            get { return (int)this.nativeInfo.cxWindowBorders; }
        }

        public int WindowBorderHeight
        {
            get { return (int) this.nativeInfo.cyWindowBorders; }
        }

        public bool IsActive
        {
           get { return this.nativeInfo.dwWindowStatus == NativeConstants.WS_ACTIVECAPTION; }
        }

        public int ProcessId
        {
            get { return this.processId; }
        }

        public IntPtr Parent
        {
            get { return this.parent; }
        }

        public IntPtr Handle
        {
            get { return this.hWnd; }
        }

        public string WindowText
        {
            get { return this.windowText; }
        }

        public bool IsVisible
        {
            get { return HasStyle(WindowStyles.WS_VISIBLE); }
        }

        public bool IsFullScreen
        {
            get { return HasStyle(WindowStyles.WS_EX_WINDOWEDGE); }
        }

        public bool IsMaximized
        {
            get { return HasStyle(WindowStyles.WS_MAXIMIZE); }
        }

        public bool IsMinimized
        {
            get { return HasStyle(WindowStyles.WS_ICONIC); }
        }

        public bool IsPopupWindow
        {
            get { return HasStyle(WindowStyles.WS_POPUPWINDOW); }
        }

        public bool IsOverlappedWindow
        {
            get { return HasStyle(WindowStyles.WS_OVERLAPPEDWINDOW); }
        }

        /// <summary>
        /// Shortcut to check that this is indeed an application window...
        /// </summary>
        public bool IsApplicationWindow
        {
            get { return IsOverlappedWindow || IsPopupWindow; }
        }

        public bool HasStyle(WindowStyles styles)
        {
            WindowStyles dwStyle = (WindowStyles)this.nativeInfo.dwStyle;
            return (dwStyle & styles) == styles;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1} ({2})", processId, windowText, WindowBounds);
        }
    }
}
