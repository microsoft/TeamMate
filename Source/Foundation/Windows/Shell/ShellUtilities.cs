using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Foundation.Win32;
using Microsoft.Tools.TeamMate.Foundation.Windows.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Shell
{
    public static class ShellUtilities
    {
        private static readonly SIZE WellKnownThumbnailSize = new SIZE(256, 256);

        public static BitmapSource ExtractIcon(string filename, int index = 0)
        {
            IntPtr hIcon = NativeMethods.ExtractIcon(Process.GetCurrentProcess().Handle, Environment.ExpandEnvironmentVariables(filename), index);
            return InteropUtilities.CreateBitmapSourceFromHIconAndDispose(hIcon);
        }

        public static BitmapSource GetThumbnail(string filename)
        {
            IntPtr hbitmap = GetThumbnailHBitmap(filename);
            return InteropUtilities.CreateBitmapSourceFromHBitmapAndDispose(hbitmap);
        }

        [DebuggerStepThrough]
        private static IntPtr GetThumbnailHBitmap(string filename)
        {
            IntPtr hbitmap = IntPtr.Zero;

            try
            {
                FileTypeInfo info = FileTypeRegistry.Instance.GetInfoFromPath(filename);
                if (info != null && info.HasThumbnailProvider)
                {
                    IShellItem ppsi;
                    Guid shellItemGuid = Marshal.GenerateGuidForType(typeof(IShellItem));
                    NativeMethods.SHCreateItemFromParsingName(filename, IntPtr.Zero, shellItemGuid, out ppsi);
                    IShellItemImageFactory factory = ppsi as IShellItemImageFactory;

                    if (factory != null)
                    {
                        factory.GetImage(WellKnownThumbnailSize, SIIGBF.SIIGBF_BIGGERSIZEOK | SIIGBF.SIIGBF_THUMBNAILONLY, out hbitmap);
                    }
                }
            }
            catch (Exception)
            {
                // This is expected if the target file does not produce thumbnails... We'll just return a null pointer
            }

            return hbitmap;
        }

        public static void ForceToForeground(IntPtr hWnd)
        {
            IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();

            // See http://www.codeproject.com/Tips/76427/How-to-bring-window-to-top-with-SetForegroundWindo
            if (foregroundWindow != hWnd)
            {
                const int VK_MENU = 0x12;
                const int KEYEVENTF_EXTENDEDKEY = 1;
                const int KEYEVENTF_KEYUP = 2;

                //to unlock SetForegroundWindow we need to imitate pressing [Alt] key
                bool pressed = false;
                if ((NativeMethods.GetAsyncKeyState(VK_MENU) & 0x8000) == 0)
                {
                    pressed = true;
                    NativeMethods.keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY, 0);
                }

                NativeMethods.SetForegroundWindow(hWnd);

                if (pressed)
                {
                    NativeMethods.keybd_event(VK_MENU, 0, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
                }
            }
        }

        public static void AllowCaptureForeground(Action action)
        {
            // See http://www.shloemi.com/2012/09/solved-setforegroundwindow-win32-api-not-always-works/
            uint foregroundProcessId;
            int currentProcessId = Process.GetCurrentProcess().Id;
            uint foreThread = NativeMethods.GetWindowThreadProcessId(NativeMethods.GetForegroundWindow(), out foregroundProcessId);
            if (foregroundProcessId == Process.GetCurrentProcess().Id)
            {
                action();
            }
            else
            {
                var appThread = NativeMethods.GetCurrentThreadId();
                bool threadsAttached = NativeMethods.AttachThreadInput(foreThread, appThread, true);
                NativeMethods.AllowSetForegroundWindow(currentProcessId);

                try
                {
                    action();
                }
                finally
                {
                    if (threadsAttached)
                    {
                        NativeMethods.AttachThreadInput(foreThread, appThread, false);
                    }
                }
            }
        }

        public static bool IsInMetroMode()
        {
            bool inMetroMode = false;

            if (Environment.OSVersion.IsWindows8OrGreater())
            {
                IAppVisibility appVisibility = new IAppVisibility();
                inMetroMode = appVisibility.IsLauncherVisible();

                if (!inMetroMode)
                {
                    NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
                    {
                        MONITOR_APP_VISIBILITY visibility = appVisibility.GetAppVisibilityOnMonitor(hMonitor);
                        inMetroMode = (visibility == MONITOR_APP_VISIBILITY.MAV_APP_VISIBLE);

                        return !inMetroMode;
                    }, IntPtr.Zero);
                }
            }

            return inMetroMode;
        }

        public static void ToggleDesktop()
        {
            GetShell().ToggleDesktop();
        }

        public static void MinimizeAll()
        {
            GetShell().MinimizeAll();
        }

        public static void UndoMinimizeAll()
        {
            GetShell().UndoMinimizeALL();
        }

        private static Shell32.Shell GetShell()
        {
            return new Shell32.Shell();
        }
    }
}
