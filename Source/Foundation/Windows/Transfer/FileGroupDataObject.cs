using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Native;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Permissions;
using System.Windows.Forms;
using IDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Transfer
{
    public class FileGroupDataObject : DataObject, IDataObject
    {
        private static readonly string[] SupportedFormats = { 
            CustomDataFormats.FileGroupDescriptorW, CustomDataFormats.FileContents, CustomDataFormats.PerformedDropEffect
        };

        private const TYMED SupportedTymeds =
            TYMED.TYMED_HGLOBAL | TYMED.TYMED_ISTREAM | TYMED.TYMED_GDI | TYMED.TYMED_MFPICT | TYMED.TYMED_ENHMF;

        private const int DV_E_TYMED = unchecked((int)0x80040069);

        private int currentItemIndex;

        public FileGroupDataObject(FileGroup fileGroup)
        {
            Assert.ParamIsNotNull(fileGroup, "fileGroup");

            this.FileGroup = fileGroup;
        }

        public FileGroup FileGroup { get; private set; }

        public override string[] GetFormats(bool autoConvert)
        {
            return SupportedFormats;
        }

        public override bool GetDataPresent(string format, bool autoConvert)
        {
            return GetFormats(autoConvert).Contains(format);
        }

        public override object GetData(string format, bool autoConvert)
        {
            if (format == CustomDataFormats.FileGroupDescriptorW)
            {
                return GetFileDescriptorStream();
            }
            else if (format == CustomDataFormats.FileContents)
            {
                return GetFileContents();
            }

            return base.GetData(format, autoConvert);
        }

        void IDataObject.GetData(ref FORMATETC formatetc, out STGMEDIUM medium)
        {
            if (formatetc.cfFormat == (short)DataFormats.GetFormat(CustomDataFormats.FileContents).Id)
            {
                currentItemIndex = formatetc.lindex;
            }

            medium = new STGMEDIUM();
            if (Supports(formatetc.tymed))
            {
                if ((formatetc.tymed & TYMED.TYMED_HGLOBAL) != TYMED.TYMED_NULL)
                {
                    medium.tymed = TYMED.TYMED_HGLOBAL;
                    medium.unionmember = NativeMethods.GlobalAlloc((int)(GlobalMemoryFlags.GHND | GlobalMemoryFlags.GMEM_DDESHARE), 1);
                    if (medium.unionmember == IntPtr.Zero)
                    {
                        throw new OutOfMemoryException();
                    }

                    try
                    {
                        ((IDataObject)this).GetDataHere(ref formatetc, ref medium);
                        return;
                    }
                    catch
                    {
                        NativeMethods.GlobalFree(new HandleRef((STGMEDIUM)medium, medium.unionmember));
                        medium.unionmember = IntPtr.Zero;
                        throw;
                    }
                }

                medium.tymed = formatetc.tymed;
                ((IDataObject)this).GetDataHere(ref formatetc, ref medium);
            }
            else
            {
                Marshal.ThrowExceptionForHR(DV_E_TYMED);
            }
        }

        private static bool Supports(TYMED tymed)
        {
            return (tymed & SupportedTymeds) != TYMED.TYMED_NULL;
        }

        private Stream GetFileDescriptorStream()
        {
            return FileGroup.GetFileDescriptorStream();
        }

        private Stream GetFileContents()
        {
            MemoryStream stream = null;

            if (currentItemIndex < FileGroup.Items.Count)
            {
                FileGroupItem item = FileGroup.Items[currentItemIndex];
                stream = item.GetFileContentsStream();
            }

            return stream;
        }
    }
}
