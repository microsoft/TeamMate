// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows;
using System.Windows.Interop;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public static class InteropExtensions
    {
        public static IntPtr GetHandle(this Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            return helper.Handle;
        }

        public static System.Drawing.Rectangle ToRectangle(this Int32Rect rect)
        {
            return new System.Drawing.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Int32Rect ToInt32Rect(this System.Drawing.Rectangle rect)
        {
            return new Int32Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static IWin32Window GetWin32Window(this Window window)
        {
            return new WindowWrapper(window.GetHandle());
        }

        public static System.Windows.Forms.IWin32Window GetWinFormsWin32Window(this Window window)
        {
            return new WindowWrapper(window.GetHandle());
        }

        private class WindowWrapper : IWin32Window, System.Windows.Forms.IWin32Window
        {
            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }

            public IntPtr Handle
            {
                get { return _hwnd; }
            }

            private IntPtr _hwnd;
        }
    }
}
