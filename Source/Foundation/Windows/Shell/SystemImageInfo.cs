using System.Windows;
using System.Windows.Media;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Shell
{
    public class SystemImageInfo
    {
        private SystemImageCache parentCache;
        private SystemImageInfo iconDelegate;
        private ImageSource smallImage, largeImage, extraLargeImage, jumboImage, thumbnail;
        private string pathOrExtension;

        internal SystemImageInfo(string pathOrExtension, SystemImageCache parentCache, SystemImageInfo iconDelegate = null)
        {
            this.pathOrExtension = pathOrExtension;
            this.parentCache = parentCache;
            this.iconDelegate = iconDelegate;
        }

        public ImageSource GetPreferredImage(Size size, out bool isThumbnail)
        {
            isThumbnail = false;
            ImageSource result = null;

            if (result == null && IsBiggerThan(size, SystemImageSizes.ExtraLargeSize))
            {
                // TODO: Sometimes, some images that don't have a Jumbo image for example, are returning
                // a "calculated" jumbo from e.g. the small icon. This looks bad. Find a way of skipping jumbo if
                // there is truly no jumbo... (e.g. .mbf file)
                if (Thumbnail != null)
                {
                    isThumbnail = true;
                    result = Thumbnail;
                }
                else
                {
                    result = JumboImage;
                }
            }

            if (result == null && IsBiggerThan(size, SystemImageSizes.LargeSize))
            {
                result = ExtraLargeImage;
            }

            if (result == null && IsBiggerThan(size, SystemImageSizes.SmallSize))
            {
                result = LargeImage;
            }

            if (result == null)
            {
                result = SmallImage;
            }

            return result;
        }

        public ImageSource SmallImage
        {
            get { return GetImage(SystemImageSize.Small, ref smallImage); }
        }

        public ImageSource LargeImage
        {
            get { return GetImage(SystemImageSize.Large, ref largeImage); }
        }

        public ImageSource ExtraLargeImage
        {
            get { return GetImage(SystemImageSize.ExtraLarge, ref extraLargeImage); }
        }

        public ImageSource JumboImage
        {
            get { return GetImage(SystemImageSize.Jumbo, ref jumboImage); }
        }

        public ImageSource Thumbnail
        {
            get { return GetImage(SystemImageSize.Thumbnail, ref thumbnail); }
        }

        private ImageSource GetImage(SystemImageSize size, ref ImageSource image)
        {
            if (image != null)
            {
                return image;
            }

            if (iconDelegate != null)
            {
                switch (size)
                {
                    case SystemImageSize.Small: return iconDelegate.SmallImage;
                    case SystemImageSize.Large: return iconDelegate.LargeImage;
                    case SystemImageSize.ExtraLarge: return iconDelegate.ExtraLargeImage;
                    case SystemImageSize.Jumbo: return iconDelegate.JumboImage;
                }
            }

            if (image == null)
            {
                image = parentCache.LoadImage(pathOrExtension, size);
            }

            return image;
        }

        private static bool IsBiggerThan(Size size, Size biggerThan)
        {
            return size.Width > biggerThan.Width || size.Height > biggerThan.Height;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// See http://msdn.microsoft.com/en-us/library/windows/desktop/bb762185(v=vs.85).aspx for more info
    /// </remarks>
    public enum SystemImageSize
    {
        /// <summary>
        /// These images are the Shell standard small icon size of 16x16, but the size can be customized by the user.
        /// </summary>
        Small,

        /// <summary>
        /// The image size is normally 32x32 pixels. However, if the Use large icons option is selected from the Effects 
        /// section of the Appearance tab in Display Properties, the image is 48x48 pixels.
        /// </summary>
        Large,

        /// <summary>
        /// These images are the Shell standard extra-large icon size. This is typically 48x48, but the size can be customized by the user.
        /// </summary>
        ExtraLarge,

        /// <summary>
        /// Windows Vista and later. The image is normally 256x256 pixels.
        /// </summary>
        Jumbo,

        /// <summary>
        /// The readable thumbnail
        /// </summary>
        Thumbnail
    }

    public static class SystemImageSizes
    {
        // TODO: Ideally, these should be dynamic properties, as some of the sizes are variable and can be customized by the user
        public static readonly Size SmallSize = new Size(16, 16);
        public static readonly Size LargeSize = new Size(32, 32);
        public static readonly Size ExtraLargeSize = new Size(48, 48);
        public static readonly Size JumboSize = new Size(256, 256);
        public static readonly Size ThumbnailSize = new Size(256, 256);
    }
}
