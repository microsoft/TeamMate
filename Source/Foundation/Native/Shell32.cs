// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in shell32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("shell32.dll", EntryPoint = "ExtractIconA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
        public static extern IntPtr ExtractIcon(IntPtr hInst, string lpszExeFileName, int nIconIndex);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        /// <summary>
        /// SHGetImageList is not exported correctly in XP.  See KB316931
        /// http://support.microsoft.com/default.aspx?scid=kb;EN-US;Q316931
        /// Apparently (and hopefully) ordinal 727 isn't going to change.
        /// </summary>
        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageList(int iImageList, ref Guid riid, ref IImageList ppv);

        [DllImport("shell32.dll", EntryPoint = "#727")]
        public extern static int SHGetImageListHandle(int iImageList, ref Guid riid, ref IntPtr handle);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        public static extern void SHCreateItemFromParsingName(
            [In][MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            [In] IntPtr pbc,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out][MarshalAs(UnmanagedType.Interface, IidParameterIndex = 2)] out IShellItem ppv);
    }
}
