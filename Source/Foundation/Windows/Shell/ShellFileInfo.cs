// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Foundation.Windows.Interop;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Shell
{
    public class ShellFileInfo
    {
        private const string ExtensionPrefix = ".";

        private ImageSource smallIcon;
        private ImageSource largeIcon;
        private string path;
        private string typeName;

        protected ShellFileInfo(string path, string typeName, ImageSource smallIcon, ImageSource largeIcon)
        {
            this.path = path;
            this.typeName = typeName;
            this.smallIcon = smallIcon;
            this.largeIcon = largeIcon;
        }

        public ImageSource SmallIcon
        {
            get { return this.smallIcon; }
        }

        public ImageSource LargeIcon
        {
            get { return this.largeIcon; }
        }

        public virtual string Extension
        {
            get 
            {
                return (this.path.StartsWith(ExtensionPrefix)) ? this.path : System.IO.Path.GetExtension(this.path);
            }
        }

        public string Path
        {
            get 
            {
                return (this.path.StartsWith(ExtensionPrefix)) ? null : this.path;
            }
        }

        public string TypeName
        {
            get { return this.typeName; }
        }

        public static ShellFileInfo GetDefaultFileInfo()
        {
            return FromExtension(null, true);
        }

        public static ShellFolderInfo GetDefaultFolderInfo()
        {
            return (ShellFolderInfo)GetShellFileInfo(null, true, true, true);
        }

        public static ShellFileInfo FromExtension(string pathOrExtension, bool loadLargeIcon = false)
        {
            if (pathOrExtension != null)
                pathOrExtension = System.IO.Path.GetExtension(pathOrExtension);

            return GetShellFileInfo(pathOrExtension, true, false, loadLargeIcon);
        }

        public static ShellFileInfo FromFile(string path, bool loadLargeIcon = false)
        {
            return GetShellFileInfo(path, false, false, loadLargeIcon);
        }

        public static ShellFolderInfo FromDirectory(string path, bool loadLargeIcon = false)
        {
            return (ShellFolderInfo) GetShellFileInfo(path, false, true, loadLargeIcon);
        }

        public static string GetTypeName(string pathOrExtension)
        {
            SHFILEINFO info = GetFileInfo(pathOrExtension, FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL, 
                SHGFI.SHGFI_TYPENAME | SHGFI.SHGFI_USEFILEATTRIBUTES);

            return info.szTypeName;
        }

        private static ShellFileInfo GetShellFileInfo(string pathOrExtension, bool useFileAttributes, bool isFolder, bool loadLargeIcon)
        {
            SHGFI flags = SHGFI.SHGFI_ICON;
            FILE_ATTRIBUTE fileAttribute = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;

            if (useFileAttributes)
            {
                flags |= SHGFI.SHGFI_USEFILEATTRIBUTES;
                if (isFolder)
                    fileAttribute = FILE_ATTRIBUTE.FILE_ATTRIBUTE_DIRECTORY;
            }

            SHFILEINFO info = GetFileInfo(pathOrExtension, fileAttribute, flags | SHGFI.SHGFI_SMALLICON | SHGFI.SHGFI_TYPENAME);

            string typeName = info.szTypeName;
            BitmapSource smallIcon = InteropUtilities.CreateBitmapSourceFromHIconAndDispose(info.hIcon);
            BitmapSource largeIcon = null;

            if (loadLargeIcon)
            {
                info = GetFileInfo(pathOrExtension, fileAttribute, flags | SHGFI.SHGFI_LARGEICON);
                largeIcon = InteropUtilities.CreateBitmapSourceFromHIconAndDispose(info.hIcon);
            }

            if (isFolder)
            {
                BitmapSource smallOpenIcon = null;
                BitmapSource largeOpenIcon = null;

                info = GetFileInfo(pathOrExtension, fileAttribute, flags | SHGFI.SHGFI_OPENICON);
                smallOpenIcon = InteropUtilities.CreateBitmapSourceFromHIconAndDispose(info.hIcon);

                if (loadLargeIcon)
                {
                    // KLUDGE: Small and large icon the same...
                    largeOpenIcon = smallOpenIcon;
                }

                return new ShellFolderInfo(pathOrExtension, typeName, smallIcon, largeIcon, smallOpenIcon, largeOpenIcon);
            }

            return new ShellFileInfo(pathOrExtension, typeName, smallIcon, largeIcon);
        }

        private static SHFILEINFO GetFileInfo(string path, FILE_ATTRIBUTE attr, SHGFI flags)
        {
            SHFILEINFO info = new SHFILEINFO();

            // Dummy filename if file has no extension...
            if ((flags & SHGFI.SHGFI_USEFILEATTRIBUTES) == SHGFI.SHGFI_USEFILEATTRIBUTES && String.IsNullOrEmpty(path))
            {
                path = "DummyFileName";
            }

            NativeMethods.SHGetFileInfo(path, (uint)attr, ref info, SHFILEINFO.Size, (uint)flags);
            return info;
        }

        public static ICollection<string> GetRegisteredFileExtensions()
        {
            // Find all the extension subkeys under HKEY_CLASSES_ROOT
            return Registry.ClassesRoot.GetSubKeyNames().Where(key => key.StartsWith(ExtensionPrefix)).ToList();
        }
    }

    public class ShellFolderInfo : ShellFileInfo
    {
        private ImageSource smallOpenIcon;
        private ImageSource largeOpenIcon;

        internal ShellFolderInfo(string path, string typeName, ImageSource smallIcon, ImageSource largeIcon, ImageSource smallOpenIcon, ImageSource largeOpenIcon)
            : base(path, typeName, smallIcon, largeIcon)
        {
            this.smallOpenIcon = smallOpenIcon;
            this.largeOpenIcon = largeOpenIcon;
        }

        public ImageSource SmallOpenIcon
        {
            get { return this.smallOpenIcon; }
        }

        public ImageSource LargeOpenIcon
        {
            get { return this.largeOpenIcon; }
        }

        public override string Extension
        {
            get
            {
                return null;
            }
        }
    }
}
