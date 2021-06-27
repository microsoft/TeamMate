using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Foundation.Win32;
using Microsoft.Tools.TeamMate.Foundation.Windows.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Shell
{
    public class SystemImageCache
    {
        private static SystemImageCache singleton;

        private SystemImageList small, large, extraLarge, jumbo;

        private Dictionary<string, SystemImageInfo> extensionBasedCache = new Dictionary<string, SystemImageInfo>(StringComparer.OrdinalIgnoreCase);

        public static SystemImageCache Instance
        {
            get
            {
                if (singleton == null)
                {
                    singleton = new SystemImageCache();
                }

                return singleton;
            }
        }

        internal SystemImageList GetImageList(SystemImageSize imageSize)
        {
            switch (imageSize)
            {
                case SystemImageSize.Small: return GetImageList(SHIL.SHIL_SMALL, ref small);
                case SystemImageSize.Large: return GetImageList(SHIL.SHIL_LARGE, ref large);
                case SystemImageSize.ExtraLarge: return GetImageList(SHIL.SHIL_EXTRALARGE, ref extraLarge);
                case SystemImageSize.Jumbo: return GetImageList(SHIL.SHIL_JUMBO, ref jumbo);

                default:
                    throw new NotSupportedException("Unsupported image list for image size: " + imageSize);
            }
        }

        private SystemImageList GetImageList(SHIL imageSize, ref SystemImageList result)
        {
            if (result == null)
            {
                result = new SystemImageList(imageSize);
            }

            return result;
        }

        public SystemImageInfo GetSystemImage(string pathOrExtension)
        {
            string extension = Path.GetExtension(pathOrExtension);
            bool inputIsPath = (extension.Length != pathOrExtension.Length);

            bool isShared = true;
            bool hasCustomThumbnail = false;

            if (inputIsPath)
            {
                string path = pathOrExtension;

                if (!Path.IsPathRooted(path))
                {
                    throw new ArgumentException("Input path must be an extension or an absolute path");
                }

                // Optimized variables and logic to do file system checks once and in order of effectiveness
                FileTypeInfo info = FileTypeRegistry.Instance.GetInfo(extension);

                bool hasCustomIcon = (info != null && info.HasCustomIcon);
                bool hasThumbnailProvider = (info != null && info.HasThumbnailProvider);
                bool fileExists = (hasCustomIcon || hasThumbnailProvider) ? File.Exists(path) : false;

                if (fileExists && hasCustomIcon)
                {
                    // NOT SHARED: Each image type for this file might be unique
                    isShared = false;
                }
                else if (fileExists && hasThumbnailProvider)
                {
                    // Icons are shared, but the image can render as a thumbnail
                    hasCustomThumbnail = true;
                }
                else if (Directory.Exists(path))
                {
                    // NOT SHARED: It is a folder, and folders might have unique icons
                    isShared = false;
                }
            }

            if (!isShared)
            {
                return new SystemImageInfo(pathOrExtension, this);
            }
            else if (hasCustomThumbnail)
            {
                SystemImageInfo iconDelegate = GetSharedImage(extension);
                return new SystemImageInfo(pathOrExtension, this, iconDelegate);
            }
            else
            {
                return GetSharedImage(extension);
            }
        }

        private SystemImageInfo GetSharedImage(string extension)
        {
            SystemImageInfo info;
            if (!extensionBasedCache.TryGetValue(extension, out info))
            {
                info = new SystemImageInfo(extension, this);
                extensionBasedCache[extension] = info;
            }
            return info;
        }

        public void RemoveFromCache(string extension)
        {
            extensionBasedCache.Remove(extension);
        }

        public ImageSource LoadImage(string pathOrExtension, SystemImageSize size)
        {
            string extension = Path.GetExtension(pathOrExtension);
            bool inputIsPath = (extension.Length != pathOrExtension.Length);

            if (size != SystemImageSize.Thumbnail)
            {
                SystemImageList imageList = GetImageList(size);

                int iconIndex = 0;

                if (!String.IsNullOrEmpty(pathOrExtension))
                {
                    bool forceLoadFromDisk = inputIsPath;
                    // TODO: Do in background thread when forceLoadFromDisk? That avoids blocking the UI
                    iconIndex = imageList.IconIndex(pathOrExtension, forceLoadFromDisk);
                }

                if (iconIndex >= 0)
                {
                    IntPtr iconHandle = imageList.IconHandle(iconIndex);
                    if (iconHandle != IntPtr.Zero)
                    {
                        return InteropUtilities.CreateBitmapSourceFromHIcon(iconHandle);
                    }
                }
            }
            else
            {
                if (inputIsPath)
                {
                    return ShellUtilities.GetThumbnail(pathOrExtension);
                }
            }

            return null;
        }
    }
}
