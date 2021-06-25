using Microsoft.Internal.Tools.TeamMate.Foundation.Native;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Shell
{
    internal class SystemImageList
    {
        private IntPtr hIml;
        private IImageList iImageList;
        private bool disposed;

        /// <summary>
        /// Creates a SystemImageList with the specified size
        /// </summary>
        /// <param name="size">Size of System ImageList</param>
        public SystemImageList(SHIL size)
        {
            this.ImageListSize = size;
            Create();
        }

        /// <summary>
        /// Gets the hImageList handle
        /// </summary>
        public IntPtr Handle
        {
            get
            {
                return this.hIml;
            }
        }

        /// <summary>
        /// Gets/sets the size of System Image List to retrieve.
        /// </summary>
        public SHIL ImageListSize 
        { 
            get; private set; 
        }

        /// <summary>
        /// Returns the size of the Image List Icons.
        /// </summary>
        public Size Size
        {
            get
            {
                int cx = 0;
                int cy = 0;

                if (iImageList == null)
                {
                    NativeMethods.ImageList_GetIconSize(hIml, ref cx, ref cy);
                }
                else
                {
                    iImageList.GetIconSize(ref cx, ref cy);
                }

                return new Size(cx, cy);
            }
        }

        /// <summary>
        /// Returns a GDI+ copy of the icon from the ImageList
        /// at the specified index.
        /// </summary>
        /// <param name="index">The index to get the icon for</param>
        /// <returns>The specified icon</returns>
        public Icon Icon(int index)
        {
            IntPtr hIcon = IconHandle(index);
            return (hIcon != IntPtr.Zero)? System.Drawing.Icon.FromHandle(hIcon) : null;
        }

        public IntPtr IconHandle(int index)
        {
            IntPtr hIcon = IntPtr.Zero;
            if (iImageList == null)
            {
                hIcon = NativeMethods.ImageList_GetIcon(hIml, index, (int)IMAGELISTDRAWFLAGS.ILD_TRANSPARENT);
            }
            else
            {
                iImageList.GetIcon(index, (int)IMAGELISTDRAWFLAGS.ILD_TRANSPARENT, ref hIcon);
            }
            return hIcon;
        }

        /// <summary>
        /// Returns the index of the icon for the specified file
        /// </summary>
        /// <param name="fileName">Filename to get icon for</param>
        /// <param name="forceLoadFromDisk">If True, then hit the disk to get the icon,
        /// otherwise only hit the disk if no cached icon is available.</param>
        /// <param name="iconState">Flags specifying the state of the icon
        /// returned.</param>
        /// <returns>Index of the icon</returns>
        public int IconIndex(string fileName, bool forceLoadFromDisk = false, SHGFI iconState = SHGFI.SHGFI_LARGEICON)
        {
            SHGFI dwFlags = SHGFI.SHGFI_SYSICONINDEX;
            FILE_ATTRIBUTE dwAttr;
            if (ImageListSize == SHIL.SHIL_SMALL)
            {
                dwFlags |= SHGFI.SHGFI_SMALLICON;
            }

            // We can choose whether to access the disk or not. If you don't
            // hit the disk, you may get the wrong icon if the icon is
            // not cached. Also only works for files.
            if (!forceLoadFromDisk)
            {
                dwFlags |= SHGFI.SHGFI_USEFILEATTRIBUTES;
                dwAttr = FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL;
            }
            else
            {
                dwAttr = 0;
            }

            // sFileSpec can be any file. You can specify a
            // file that does not exist and still get the
            // icon, for example sFileSpec = "C:\PANTS.DOC"
            SHFILEINFO shfi = new SHFILEINFO();
            IntPtr retVal = NativeMethods.SHGetFileInfo(fileName, (uint)dwAttr, ref shfi, SHFILEINFO.Size, ((uint)(dwFlags) | (uint)iconState));

            if (retVal.Equals(IntPtr.Zero))
            {
                System.Diagnostics.Debug.Assert((!retVal.Equals(IntPtr.Zero)), "Failed to get icon index");
                return 0;
            }
            else
            {
                return shfi.iIcon;
            }
        }

        /// <summary>
        /// Creates the SystemImageList
        /// </summary>
        private void Create()
        {
            // forget last image list if any:
            hIml = IntPtr.Zero;

            bool supportsIImageList = Environment.OSVersion.IsWindowsXPOrGreater();

            if (supportsIImageList)
            {
                // Get the System IImageList object from the Shell:
                Guid iidImageList = Marshal.GenerateGuidForType(typeof(IImageList));
                int ret = NativeMethods.SHGetImageList((int)ImageListSize, ref iidImageList, ref iImageList);

                // the image list handle is the IUnknown pointer, but 
                // using Marshal.GetIUnknownForObject doesn't return
                // the right value.  It really doesn't hurt to make
                // a second call to get the handle:
                NativeMethods.SHGetImageListHandle((int)ImageListSize, ref iidImageList, ref hIml);
            }
            else
            {
                // Prepare flags:
                SHGFI dwFlags = SHGFI.SHGFI_USEFILEATTRIBUTES | SHGFI.SHGFI_SYSICONINDEX;
                if (ImageListSize == SHIL.SHIL_SMALL)
                {
                    dwFlags |= SHGFI.SHGFI_SMALLICON;
                }

                // Get image list
                SHFILEINFO shfi = new SHFILEINFO();

                // Call SHGetFileInfo to get the image list handle
                // using an arbitrary file:
                hIml = NativeMethods.SHGetFileInfo(".txt", (uint)FILE_ATTRIBUTE.FILE_ATTRIBUTE_NORMAL, ref shfi, SHFILEINFO.Size, (uint)dwFlags);
                System.Diagnostics.Debug.Assert((hIml != IntPtr.Zero), "Failed to create Image List");
            }
        }

        /// <summary>
        /// Clears up any resources associated with the SystemImageList
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clears up any resources associated with the SystemImageList
        /// when disposing is true.
        /// </summary>
        /// <param name="disposing">Whether the object is being disposed</param>
        public virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (iImageList != null)
                    {
                        Marshal.ReleaseComObject(iImageList);
                    }

                    iImageList = null;
                }
            }

            disposed = true;
        }

        /// <summary>
        /// Finalise for SysImageList
        /// </summary>
        ~SystemImageList()
        {
            Dispose(false);
        }
    }
}
