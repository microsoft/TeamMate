using System;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Exposes PInvoke method wrappers for functions in gdi32.dll.
    /// </summary>
    public static partial class NativeMethods
    {
        [DllImport("gdi32.dll", SetLastError = true)]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, PreserveSig = true, SetLastError = true)]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, RasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice, string lpszOutput, IntPtr lpInitData);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

        [DllImport("gdi32.dll")]
        public static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest,
            int nWidthDest, int nHeightDest,
            IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
            RasterOperations dwRop);

        [DllImport("gdi32.dll")]
        public static extern bool SetWorldTransform(IntPtr hdc, [In] ref XFORM lpXform);

        [DllImport("gdi32.dll")]
        public static extern int SetGraphicsMode(IntPtr hdc, GM iMode);

        [DllImport("gdi32.dll")]
        public static extern int SetStretchBltMode(IntPtr hdc, STRETCH iStretchMode);

        /// <summary>
        /// The GetDeviceCaps function retrieves device-specific information for the specified device.
        /// </summary>
        /// <param name="hdc">A handle to the DC.</param>
        /// <param name="nIndex">The item to be returned.</param>
        /// <returns>The return value specifies the value of the desired item.</returns>
        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        /// <summary>
        /// Gets the current system DPI setting.
        /// </summary>
        /// <returns>The system DPI setting.</returns>
        public static void GetSystemDpi(out int dpiX, out int dpiY)
        {
            IntPtr hDC = GetDC(IntPtr.Zero);

            try
            {
                dpiX = GetDeviceCaps(hDC, (int) SystemMetric.SM_LOGPIXELSX);
                dpiY = GetDeviceCaps(hDC, (int) SystemMetric.SM_LOGPIXELSY);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, hDC);
            }
        }
    }

    public struct XFORM
    {
        public float eM11;
        public float eM12;
        public float eM21;
        public float eM22;
        public float eDx;
        public float eDy;
    }

    public enum GM : uint
    { 
        GM_COMPATIBLE = 1,
        GM_ADVANCED = 2,
    }

    public enum STRETCH
    {
        STRETCH_ANDSCANS = 1,
        STRETCH_ORSCANS = 2,
        STRETCH_DELETESCANS = 3,
        STRETCH_HALFTONE = 4,
    }
}
