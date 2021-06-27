// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Media.Imaging
{
    public static class BitmapUtilities
    {
        public static readonly ICollection<string> SupportedImageExtensions = new string[] {
            ".bmp",".gif",".jpg", ".jpeg", ".png", ".tif", ".tiff", ".wdp",
        };

        private static readonly ISet<string> SupportedImageExtensionsSet =
            new HashSet<string>(BitmapUtilities.SupportedImageExtensions, StringComparer.OrdinalIgnoreCase);

        public static bool IsSupportedImageFile(string filename)
        {
            string extension = Path.GetExtension(filename);
            return (extension != null && SupportedImageExtensionsSet.Contains(extension));
        }

        public static BitmapImage LoadImage(string filename)
        {
            return LoadImage(new Uri(filename));
        }

        public static BitmapImage LoadImage(Uri uriSource)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = uriSource;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.EndInit();
            image.Freeze();
            return image;
        }

        /// <summary>
        /// Loads a bitmap image from a stream. The bitmap image is frozen.
        /// </summary>
        /// <param name="stream">A stream.</param>
        /// <returns>The loaded image.</returns>
        public static BitmapImage LoadImage(Stream stream)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.None;
            image.EndInit();
            image.Freeze();
            return image;
        }

        public static BitmapEncoder CreateEncoder(string filename)
        {
            string extension = Path.GetExtension(filename);
            if (extension != null)
                extension = extension.ToLowerInvariant();

            switch (extension)
            {
                case ".jpg":
                case ".jpeg":
                    return new JpegBitmapEncoder();

                case ".bmp":
                    return new BmpBitmapEncoder();

                case ".gif":
                    return new GifBitmapEncoder();

                case ".tif":
                case ".tiff":
                    return new TiffBitmapEncoder();

                case ".png":
                    return new PngBitmapEncoder();

                case ".wdp":
                    return new WmpBitmapEncoder();
            }

            throw new ArgumentException("Could not figure out encoder for filename " + filename);
        }

        public static void Save(BitmapSource bitmapSource, string filename)
        {
            BitmapEncoder encoder = CreateEncoder(filename);
            using (Stream stream = File.Create(filename))
            {
                Save(bitmapSource, encoder, stream);
            }
        }

        public static void SaveAsJpeg(BitmapSource bitmapSource, Stream stream)
        {
            BitmapEncoder encoder = new JpegBitmapEncoder();
            Save(bitmapSource, encoder, stream);
        }

        public static void SaveAsPng(BitmapSource bitmapSource, Stream stream)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            Save(bitmapSource, encoder, stream);
        }

        private static void Save(BitmapSource bitmapSource, BitmapEncoder encoder, Stream stream)
        {
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            encoder.Save(stream);
        }

        public static RenderTargetBitmap CaptureBitmap(FrameworkElement element)
        {
            // Some DPI magic to render bitmaps nicely when the Windows "text size DPI settings" are different
            // from the standard 100% / 96pdi
            double bitmapDpi = 96;

            int dpiX, dpiY;
            NativeMethods.GetSystemDpi(out dpiX, out dpiY);

            FrameworkElement contentElement = UnwrapForBitmapCapture(element);
            double width = contentElement.ActualWidth * (dpiX / bitmapDpi);
            double height = contentElement.ActualHeight * (dpiY / bitmapDpi);

            VisualBrush elementBrush = new VisualBrush(contentElement);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext dc = visual.RenderOpen())
            {
                dc.DrawRectangle(elementBrush, null, new Rect(0, 0, width, height));
            }

            int pixelWidth = (int)Math.Round(width);
            int pixelHeight = (int)Math.Round(height);
            RenderTargetBitmap bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, bitmapDpi, bitmapDpi, PixelFormats.Default);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        private static FrameworkElement UnwrapForBitmapCapture(FrameworkElement element)
        {
            // ContentPresenters have to be handled specially, as rendering them themselves will not yield the results
            // we want... We want the first visual under them instead...
            if (element is ContentPresenter)
            {
                int childrenCount = VisualTreeHelper.GetChildrenCount(element);
                if (childrenCount == 1)
                {
                    FrameworkElement result = VisualTreeHelper.GetChild(element, 0) as FrameworkElement;
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return element;
        }

        public static BitmapSource CaptureUnsourcedBitmap(FrameworkElement container)
        {
            // Measure and arrange the container 
            container.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            container.Arrange(new Rect(container.DesiredSize));

            // Temporarily add a PresentationSource if none exists 
            using (var temporaryPresentationSource = new HwndSource(new HwndSourceParameters()) { RootVisual = container })
            {
                // Flush the dispatcher queue 
                container.Dispatcher.Invoke(DispatcherPriority.SystemIdle, new Action(() => { }));
                return BitmapUtilities.CaptureBitmap(container);
            }
        }


        /// <summary>
        /// Scales a bitmap if needed to fix a maximum with and a maximum height. The bitmap is scaled
        /// proportionally across both axes if needed.
        /// </summary>
        /// <param name="image">A bitmap to scale.</param>
        /// <param name="maxPixelWidth">The maximum width the bitmap should have.</param>
        /// <param name="maxPixelHeight">The maximum height the bitmap should have.</param>
        /// <returns>The scaled bitmap, or the original bitmap if no scaling was required.</returns>
        public static BitmapSource ScaleBitmapToFit(BitmapSource image, int maxPixelWidth, int maxPixelHeight)
        {
            if (maxPixelWidth <= 0)
            {
                throw new ArgumentOutOfRangeException("maxPixelWidth");
            }

            if (maxPixelHeight <= 0)
            {
                throw new ArgumentOutOfRangeException("maxPixelHeight");
            }

            // If image is bigger than expected target, scale it down...
            if (image.PixelWidth > maxPixelWidth || image.PixelHeight > maxPixelHeight)
            {
                double scaleFactor = GetScaleFactor(image, maxPixelWidth, maxPixelHeight);
                image = ScaleBitmap(image, scaleFactor);
            }

            return image;
        }

        public static void GetLogicalSize(BitmapSource image, out double width, out double height)
        {
            int xPixels, yPixels;
            NativeMethods.GetSystemDpi(out xPixels, out yPixels);

            width = (image.PixelWidth * xPixels / image.DpiX);
            height = (image.PixelHeight * yPixels / image.DpiY);
        }


        public static double GetScaleFactor(BitmapSource image, double maxPixelWidth, double maxPixelHeight)
        {
            double width, height;
            GetLogicalSize(image, out width, out height);

            return GetScaleFactor(width, height, maxPixelWidth, maxPixelHeight);
        }

        public static double GetScaleFactor(double width, double height, double maxWidth, double maxHeight)
        {
            double scaleX = (width > 0 && maxWidth > 0 && width > maxWidth) ? maxWidth / width : 1.0;
            double scaleY = (height > 0 && maxHeight > 0 && height > maxHeight) ? maxHeight / height : 1.0;
            return Math.Min(scaleX, scaleY);
        }

        /// <summary>
        /// Scales a bitmap using a simple and fast scaling algorithm.
        /// </summary>
        /// <param name="bitmap">A bitmap to scale.</param>
        /// <param name="scaleFactor">The scale factor to apply.</param>
        /// <returns>The scaled bitmap.</returns>
        public static BitmapSource ScaleBitmap(BitmapSource bitmap, double scaleFactor)
        {
            if (scaleFactor <= 0)
            {
                throw new ArgumentOutOfRangeException("scaleFactor");
            }

            return new TransformedBitmap(bitmap, new ScaleTransform(scaleFactor, scaleFactor));
        }

        /// <summary>
        /// Scales a bitmap using a advanced and scaling algorithms (which can be customized using a BitmapScalingMode).
        /// </summary>
        /// <param name="bitmap">A bitmap to scale.</param>
        /// <param name="scaleFactor">The scale factor to apply.</param>
        /// <param name="scalingMode">The scaling mode to apply.</param>
        /// <returns>The scaled bitmap.</returns>
        public static BitmapSource ScaleBitmapAdvanced(BitmapSource bitmap, double scaleFactor, BitmapScalingMode scalingMode = BitmapScalingMode.Unspecified)
        {
            int width = (int)(bitmap.PixelWidth * scaleFactor);
            int height = (int)(bitmap.PixelHeight * scaleFactor);

            DrawingGroup group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, scalingMode);
            group.Children.Add(new ImageDrawing(bitmap, new Rect(0, 0, width, height)));

            DrawingVisual targetVisual = new DrawingVisual();
            using (DrawingContext targetContext = targetVisual.RenderOpen())
            {
                targetContext.DrawDrawing(group);
            }

            RenderTargetBitmap target = new RenderTargetBitmap(width, height, bitmap.DpiX, bitmap.DpiY, PixelFormats.Default);
            target.Render(targetVisual);
            return BitmapFrame.Create(target);
        }
    }
}
