using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Win32.SafeHandles;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Interop
{
    public static class InteropUtilities
    {
        public static Cursor CursorFromBitmap(Bitmap bmp)
        {
            return CursorFromBitmap(bmp, bmp.Width / 2, bmp.Height / 2);
        }

        public static Cursor CursorFromBitmap(Bitmap bmp, int xHotSpot, int yHotSpot)
        {
            ICONINFO iconInfo = new ICONINFO();
            NativeMethods.GetIconInfo(bmp.GetHicon(), ref iconInfo);
            iconInfo.fIcon = false;
            iconInfo.xHotspot = xHotSpot;
            iconInfo.yHotspot = yHotSpot;

            IntPtr hIcon = NativeMethods.CreateIconIndirect(ref iconInfo);
            SafeFileHandle handle = new SafeFileHandle(hIcon, true);
            return CursorInteropHelper.Create(handle);
        }

        public static Cursor CursorFromBitmapSource(BitmapSource bitmapSource)
        {
            var bmp = InteropUtilities.BitmapFromBitmapSource(bitmapSource);
            return CursorFromBitmap(bmp);
        }

        public static Cursor CursorFromBitmapSource(BitmapSource bitmapSource, int xHotSpot, int yHotSpot)
        {
            var bmp = InteropUtilities.BitmapFromBitmapSource(bitmapSource);
            return CursorFromBitmap(bmp, xHotSpot, yHotSpot);
        }

        public static Bitmap BitmapFromBitmapSource(BitmapSource source)
        {
            var format = System.Drawing.Imaging.PixelFormat.Format32bppPArgb;
            Bitmap bmp = new Bitmap(source.PixelWidth, source.PixelHeight, format);
            BitmapData data = bmp.LockBits(new Rectangle(System.Drawing.Point.Empty, bmp.Size), ImageLockMode.WriteOnly, format);
            source.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static Icon IconFromBitmapSource(BitmapSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            // TODO: Do we need to destroy the icon handle?
            Bitmap bitmap = BitmapFromBitmapSource(source);
            return Icon.FromHandle(bitmap.GetHicon());
        }

        public static Icon IconFromWindowIcon(IntPtr hWnd, WindowIconType type)
        {
            IntPtr iconHandle = GetWindowIcon(hWnd, type);
            return IconFromHandleAndDispose(iconHandle);
        }

        public static Icon IconFromHandleAndDispose(IntPtr iconHandle)
        {
            Icon icon = null;

            if (iconHandle != IntPtr.Zero)
            {
                try
                {
                    icon = Icon.FromHandle(iconHandle);
                }
                finally
                {
                    NativeMethods.DestroyIcon(iconHandle);
                }
            }

            return icon;
        }

        public static BitmapSource CreateBitmapSourceFromHIcon(IntPtr iconHandle)
        {
            BitmapSource bitmap = Imaging.CreateBitmapSourceFromHIcon(iconHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapSource CreateBitmapSourceFromHBitmap(IntPtr hBitmap, IntPtr palette, Int32Rect sourceRect, BitmapSizeOptions sizeOptions)
        {
            BitmapSource bitmap = Imaging.CreateBitmapSourceFromHBitmap(hBitmap, palette, sourceRect, sizeOptions);
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
        {
            return CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource CreateBitmapSourceFromHBitmap(IntPtr hBitmap)
        {
            return CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource CreateBitmapSourceFromHBitmapAndDispose(IntPtr hBitmap)
        {
            return CreateBitmapSourceFromHBitmapAndDispose(hBitmap, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public static BitmapSource CreateBitmapSourceFromHBitmapAndDispose(IntPtr hBitmap, Int32Rect sourceRect, BitmapSizeOptions sizeOptions)
        {
            if (hBitmap != IntPtr.Zero)
            {
                try
                {
                    return CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, sourceRect, sizeOptions);
                }
                finally
                {
                    NativeMethods.DeleteObject(hBitmap);
                }
            }

            return null;
        }

        public static IntPtr GetWindowIcon(IntPtr hwnd, WindowIconType type)
        {
            IntPtr iconHandle = IntPtr.Zero;

            switch (type)
            {
                case WindowIconType.Small:
                    iconHandle = GetSmallWindowIcon(hwnd);
                    break;

                case WindowIconType.Large:
                    iconHandle = GetLargeWindowIcon(hwnd);
                    break;

                case WindowIconType.SmallOrAny:
                    iconHandle = GetSmallWindowIcon(hwnd);
                    if (iconHandle == IntPtr.Zero)
                    {
                        iconHandle = GetLargeWindowIcon(hwnd);
                    }
                    break;

                case WindowIconType.LargeOrAny:
                    iconHandle = GetLargeWindowIcon(hwnd);
                    if (iconHandle == IntPtr.Zero)
                    {
                        iconHandle = GetSmallWindowIcon(hwnd);
                    }

                    break;
            }

            return iconHandle;
        }

        private static IntPtr GetLargeWindowIcon(IntPtr hwnd)
        {
            IntPtr iconHandle = IntPtr.Zero;

            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, (uint)WindowsMessage.WM_GETICON, NativeConstants.ICON_BIG, 0);

            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.GetClassLongPtr(hwnd, NativeConstants.GCL_HICON);

            return iconHandle;
        }

        private static IntPtr GetSmallWindowIcon(IntPtr hwnd)
        {
            IntPtr iconHandle = IntPtr.Zero;

            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, (uint)WindowsMessage.WM_GETICON, NativeConstants.ICON_SMALL, 0);

            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.GetClassLongPtr(hwnd, NativeConstants.GCL_HICONSM);

            if (iconHandle == IntPtr.Zero)
                iconHandle = NativeMethods.SendMessage(hwnd, (uint)WindowsMessage.WM_GETICON, NativeConstants.ICON_SMALL2, 0);

            return iconHandle;
        }

        public static BitmapSource BitmapSourceFromWindowIcon(IntPtr hWnd, WindowIconType type)
        {
            IntPtr hIcon = GetWindowIcon(hWnd, type);
            if (hIcon != IntPtr.Zero)
            {
                // TODO: Does this pointer need to be disposed?
                return CreateBitmapSourceFromHIcon(hIcon);
            }

            return null;
        }

        public static BitmapSource CreateBitmapSourceFromIcon(Icon icon)
        {
            return (icon != null) ? CreateBitmapSourceFromHIconAndDispose(icon.Handle) : null;
        }

        public static BitmapSource CreateBitmapSourceFromHIconAndDispose(IntPtr iconHandle)
        {
            if (iconHandle != IntPtr.Zero)
            {
                try
                {
                    return CreateBitmapSourceFromHIcon(iconHandle);
                }
                finally
                {
                    NativeMethods.DestroyIcon(iconHandle);
                }
            }

            return null;
        }

        // Default number of device independent units (DIUs) per inch.
        // See http://msdn.microsoft.com/en-us/library/ee671605(v=vs.85).aspx for more info.
        private const int DefaultDiuPerInch = 96;

        /// <summary>
        /// Converts a screen point (from e.g. GetCursorPors) into a logical point consumable by WPF,
        /// in Device-Independent units (e.g. for setting an absolute window location).
        /// </summary>
        /// <param name="physicalPoint">A point in physical screen coordinates.</param>
        /// <returns>The screen point converted to Device-Independent Units (DIUs).</returns>
        public static Point PhysicalToLogicalPoint(System.Drawing.Point physicalPoint)
        {
            int xPixels, yPixels;
            NativeMethods.GetSystemDpi(out xPixels, out yPixels);

            if (xPixels != 0 && yPixels != 0 && (xPixels != DefaultDiuPerInch || yPixels != DefaultDiuPerInch))
            {
                double x = physicalPoint.X * DefaultDiuPerInch / xPixels;
                double y = physicalPoint.Y * DefaultDiuPerInch / yPixels;
                return new Point(x, y);
            }
            else
            {
                // If DPI is set to the default unit, there is no transformation required...
                return new Point(physicalPoint.X, physicalPoint.Y);
            }
        }

        public static System.Drawing.Point LogicalToPhysicalPoint(Point logicalPoint)
        {
            int xPixels, yPixels;
            NativeMethods.GetSystemDpi(out xPixels, out yPixels);

            if (xPixels != 0 && yPixels != 0 && (xPixels != DefaultDiuPerInch || yPixels != DefaultDiuPerInch))
            {
                double x = logicalPoint.X / (DefaultDiuPerInch / xPixels);
                double y = logicalPoint.Y / (DefaultDiuPerInch / yPixels);
                return new System.Drawing.Point((int)x, (int)y);
            }
            else
            {
                // If DPI is set to the default unit, there is no transformation required...
                return new System.Drawing.Point((int)logicalPoint.X, (int)logicalPoint.Y);
            }
        }

        public static Rect PhysicalToLogicalRectangle(System.Drawing.Rectangle rect)
        {
            var p1 = PhysicalToLogicalPoint(rect.Location);
            var p2 = PhysicalToLogicalPoint(new System.Drawing.Point(rect.X + rect.Width, rect.Y + rect.Height));
            return new Rect(p1, p2);
        }

        public static System.Drawing.Rectangle LogicalToPhysicalRectangle(Rect rect)
        {
            var p1 = LogicalToPhysicalPoint(rect.TopLeft);
            var p2 = LogicalToPhysicalPoint(rect.BottomRight);
            return new System.Drawing.Rectangle(p1.X, p1.Y, p2.X - p1.X, p2.Y - p1.Y);
        }

        public static System.Windows.Size PhysicalToLogicalSize(System.Drawing.Size physicalSize)
        {
            return PhysicalToLogicalRectangle(new Rectangle(0, 0, physicalSize.Width, physicalSize.Height)).Size;
        }

        public static System.Drawing.Size LogicalToPhysicalSize(System.Windows.Size size)
        {
            return LogicalToPhysicalRectangle(new Rect(size)).Size;
        }
    }

    public enum WindowIconType
    {
        Small,
        Large,
        SmallOrAny,
        LargeOrAny
    }
}
