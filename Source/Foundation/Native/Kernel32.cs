using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in kernel32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("kernel32.dll")]
        public static extern uint GetCurrentThreadId();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
        public static extern bool CloseHandle(HandleRef handle);

        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern SafeFileHandle CreateFile(
              string lpFileName,
              uint dwDesiredAccess,
              uint dwShareMode,
              uint SecurityAttributes,
              uint dwCreationDisposition,
              uint dwFlagsAndAttributes,
              int hTemplateFile
              );

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern SafeFileHandle CreateMailslot(string lpName, uint nMaxMessageSize,
           int lReadTimeout, SECURITY_ATTRIBUTES lpSecurityAttributes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern bool GetMailslotInfo(SafeFileHandle hMailslot,
            out int lpMaxMessageSize,
            out int lpNextSize,
            out int lpMessageCount,
            out int lpReadTimeout);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GlobalAlloc(int uFlags, int dwBytes);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GlobalFree(HandleRef handle);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);
    }
}
