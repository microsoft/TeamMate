using System;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in dwmapi.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmUnregisterThumbnail(IntPtr HThumbnail);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmUpdateThumbnailProperties(IntPtr hThumbnail, ref DWM_THUMBNAIL_PROPERTIES props);

        [DllImport("dwmapi.dll", PreserveSig = false)]
        public static extern void DwmQueryThumbnailSourceSize(IntPtr hThumbnail, out SIZE size);

        [DllImport("dwmapi.dll")]
        public static extern Int32 DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern Int32 DwmSetWindowAttribute(IntPtr hwnd, Int32 attr, ref Int32 attrValue, Int32 attrSize);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DWM_THUMBNAIL_PROPERTIES
    {
        public ThumbnailFlags dwFlags;
        public RECT rcDestination;
        public RECT rcSource;
        public byte opacity;
        public bool fVisible;
        public bool fSourceClientAreaOnly;
    }

    [Flags]
    public enum ThumbnailFlags
    {
        DWM_TNP_RECTDESTINATION = 0x00000001,
        DWM_TNP_RECTSOURCE = 0x00000002,
        DWM_TNP_OPACITY = 0x00000004,
        DWM_TNP_VISIBLE = 0x00000008,
        DWM_TNP_SOURCECLIENTAREAONLY = 0x00000010
    }
}
