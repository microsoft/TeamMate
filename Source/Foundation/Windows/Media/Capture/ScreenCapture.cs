using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Foundation.Windows.Interop;
using Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging;
using Microsoft.Tools.TeamMate.Foundation.Windows.Shell;
using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Media.Capture
{
    public static class ScreenCapture
    {
        public static BitmapSource CaptureScreen()
        {
            Int32Rect region = new Int32Rect(0, 0, (int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            return CaptureRegion(region);
        }

        public static bool SelectScreenBounds(out Int32Rect bounds)
        {
            return SelectScreenBounds(null, out bounds);
        }

        private static bool SelectScreenBounds(BitmapSource screenBitmap, out Int32Rect bounds)
        {
            return new BoundsSelectionWindow().SelectBounds(screenBitmap, out bounds);
        }

        public static BitmapSource CaptureRegionInteractively()
        {
            Int32Rect bounds;

            BitmapSource screenBitmap = null;
            bool inMetroMode = ShellUtilities.IsInMetroMode();
            if (inMetroMode)
            {
                screenBitmap = CaptureScreen();
            }

            if (SelectScreenBounds(screenBitmap, out bounds))
            {
                if (screenBitmap != null)
                {
                    return new CroppedBitmap(screenBitmap, bounds);
                }
                else
                {
                    return CaptureRegion(bounds);
                }
            }

            return null;
        }

        public static BitmapSource CaptureRegion(Int32Rect region)
        {
            // TODO: Validate that region is not bigger than virtual screen...

            Bitmap bitmap = new Bitmap(region.Width, region.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);

            using (bitmap)
            using (graphics)
            {
                graphics.CopyFromScreen(region.X, region.Y, 0, 0, bitmap.Size);
                IntPtr hBitmap = bitmap.GetHbitmap();

                try
                {
                    return InteropUtilities.CreateBitmapSourceFromHBitmapAndDispose(hBitmap);
                }
                finally
                {
                    NativeMethods.DeleteObject(hBitmap);
                }
            }
        }

        public static BitmapSource CaptureWindow(IntPtr hWnd)
        {
            return CaptureWindow(hWnd, false, null);
        }

        public static BitmapSource CaptureWindow(IntPtr hWnd, System.Drawing.Size? maxPixelSize)
        {
            return CaptureWindow(hWnd, false, maxPixelSize);
        }

        public static BitmapSource CaptureWindow(IntPtr hWnd, bool restoreMinimizedWindow)
        {
            return CaptureWindow(hWnd, restoreMinimizedWindow, null);
        }

        public static BitmapSource CaptureWindow(IntPtr hWnd, bool restoreMinimizedWindow, System.Drawing.Size? maxPixelSize)
        {
            Bitmap bitmap = CaptureWindowBitmap(hWnd, restoreMinimizedWindow, maxPixelSize);
            return InteropUtilities.CreateBitmapSourceFromBitmap(bitmap);
        }

        public static Bitmap CaptureWindowBitmap(IntPtr hWnd, bool restoreMinimizedWindow, System.Drawing.Size? maxPixelSize)
        {
            Bitmap bitmap = CaptureWindowBitmap(hWnd, restoreMinimizedWindow);

            // Step 3: Finally, create a bitmap from the captured image
            if (maxPixelSize != null)
            {
                double scaleFactor = BitmapUtilities.GetScaleFactor(bitmap.Width, bitmap.Height, maxPixelSize.Value.Width, maxPixelSize.Value.Height);
                if (scaleFactor < 1)
                {
                    Bitmap resizedBitmap = ResizeImage(bitmap, (int)Math.Round(bitmap.Width * scaleFactor), (int)Math.Round(bitmap.Height * scaleFactor));
                    bitmap.Dispose();
                    bitmap = resizedBitmap;
                }
            }

            return bitmap;
        }

        public static Bitmap CaptureWindowBitmap(IntPtr hWnd, bool restoreMinimizedWindow)
        {
            bool minimizeWindowWhenDone = false;
            int oldExtStyle = 0;

            IDisposable systemAnimationReenabler = null;

            if (NativeMethods.IsIconic(hWnd) && restoreMinimizedWindow)
            {
                systemAnimationReenabler = ExtendedSystemParameters.DisableSystemAnimations();
                RestoreWindowBeforeCapture(hWnd, out oldExtStyle);
                minimizeWindowWhenDone = true;
            }

            Bitmap bitmap;

            try
            {
                bitmap = CaptureWindowBitmap(hWnd);
            }
            finally
            {
                if (minimizeWindowWhenDone)
                {
                    MinimizeWindowAfterCapture(hWnd, oldExtStyle);

                    if (systemAnimationReenabler != null)
                    {
                        systemAnimationReenabler.Dispose();
                    }
                }
            }
            return bitmap;
        }

        private static Bitmap CaptureWindowBitmap(IntPtr hWnd)
        {
            WindowInfo windowInfo = WindowInfo.FromHwnd(hWnd);
            RECT bounds = windowInfo.WindowBounds;

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                throw new InvalidOperationException(String.Format(
                    "Cannot capture screen for window with handle {0}, the window has invalid bounds {1}", hWnd, bounds
                ));
            }

            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
            {
                // Step 1: Try to print the window on the bitmap
                IntPtr hdcBitmap = graphics.GetHdc();
                bool succeeded = NativeMethods.PrintWindow(hWnd, hdcBitmap, 0);
                graphics.ReleaseHdc(hdcBitmap);

                if (!succeeded)
                {
                    // Fill the rectangle with a solid grey color if we failed to print the window...
                    using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(System.Drawing.Color.Gray))
                    {
                        graphics.FillRectangle(brush, new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size));
                    }
                }

                // Step 2: If the window is irregular, clip the region outside of the window with a transparent color
                IntPtr hRgn = NativeMethods.CreateRectRgn(0, 0, 0, 0);
                NativeMethods.GetWindowRgn(hWnd, hRgn);
                using (System.Drawing.Region region = System.Drawing.Region.FromHrgn(hRgn))
                {
                    if (!region.IsEmpty(graphics))
                    {
                        graphics.ExcludeClip(region);
                        graphics.Clear(System.Drawing.Color.Transparent);
                    }
                }

                NativeMethods.DeleteObject(hRgn);
            }

            // HACK: The actual window bounds are greater than what is shown on screen (e.g. maximized windows
            // have an 8 pixel grey border around them). Crop any excess from the bitmap itself.
            bitmap = CropWindowExcess(bitmap, windowInfo);

            return bitmap;
        }

        private static Bitmap CropBitmap(Bitmap bmpImage, Rectangle cropArea)
        {
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            Bitmap resizedBitmap = new Bitmap(width, height);

            // set the resolutions the same to avoid cropping due to resolution differences
            resizedBitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (Graphics graphics = Graphics.FromImage(resizedBitmap))
            {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                graphics.DrawImage(image, 0, 0, resizedBitmap.Width, resizedBitmap.Height);
            }

            return resizedBitmap;
        }

        /// <summary>
        /// Crops any undesired excess in the captured window bounds to remove unwanted gray borders.
        /// GetWindowRect() returns excessive boundaries in cases where e.g. the window is maximized
        /// and so forth.
        /// </summary>
        private static Bitmap CropWindowExcess(Bitmap input, WindowInfo windowInfo)
        {
            if (windowInfo.IsMaximized)
            {
                if (windowInfo.WindowBorderWidth > 0 || windowInfo.WindowBorderHeight > 0)
                {
                    int newX = windowInfo.WindowBorderWidth;
                    int newY = windowInfo.WindowBorderHeight;
                    int cropWidth = input.Width - windowInfo.WindowBorderWidth * 2;
                    int cropHeight = input.Height - windowInfo.WindowBorderHeight * 2;

                    Rectangle cropArea = new Rectangle(newX, newY, cropWidth, cropHeight);
                    Bitmap croppedBitmap = CropBitmap(input, cropArea);
                    input.Dispose();
                    return croppedBitmap;
                }
            }

            return input;
        }

        private static void RestoreWindowBeforeCapture(IntPtr hWnd, out int oldExtStyle)
        {
            oldExtStyle = NativeMethods.GetWindowLong(hWnd, GetWindowLong.GWL_EXSTYLE);
            NativeMethods.SetWindowLong(hWnd, GetWindowLong.GWL_EXSTYLE, oldExtStyle | (int)WindowStyles.WS_EX_LAYERED);
            NativeMethods.SetLayeredWindowAttributes(hWnd, 0, 1, NativeConstants.LWA_ALPHA);

            NativeMethods.ShowWindow(hWnd, ShowWindow.SW_RESTORE);
            NativeMethods.SendMessage(hWnd, (uint)WindowsMessage.WM_PAINT, 0, 0);

            // TODO: Need to wait some time until restored windows is nicely painted. Should this be a parameter too?
            Thread.Sleep(200);
        }

        private static void MinimizeWindowAfterCapture(IntPtr hWnd, int oldExtStyle)
        {
            NativeMethods.ShowWindow(hWnd, ShowWindow.SW_MINIMIZE);
            NativeMethods.SetWindowLong(hWnd, GetWindowLong.GWL_EXSTYLE, oldExtStyle);
        }
    }
}
