using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in comctl32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("comctl32.dll")]
        public extern static int ImageList_Draw(IntPtr hIml, int i, IntPtr hdcDst, int x, int y, int fStyle);

        [DllImport("comctl32.dll")]
        public extern static int ImageList_DrawIndirect(ref IMAGELISTDRAWPARAMS pimldp);

        [DllImport("comctl32.dll")]
        public extern static int ImageList_GetIconSize(IntPtr himl, ref int cx, ref int cy);

        [DllImport("comctl32.dll")]
        public extern static IntPtr ImageList_GetIcon(IntPtr himl, int i, int flags);
    }
}
