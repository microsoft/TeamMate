using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Foundation.Native;
using Microsoft.Tools.TeamMate.Foundation.Text;
using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Windows;
using ComDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;
using IDataObject = System.Windows.IDataObject;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Transfer
{
    public static class DataObjectExtensions
    {
        private const int SizeOfFileDescriptorW = 592;
        private const int OffsetToFileName = 72;

        public static bool ContainsFileGroup(this IDataObject dataObject)
        {
            Assert.ParamIsNotNull(dataObject, "dataObject");

            return dataObject.GetDataPresent(CustomDataFormats.FileGroupDescriptorW);
        }

        public static FileGroup GetFileGroup(this IDataObject dataObject)
        {
            Assert.ParamIsNotNull(dataObject, "dataObject");

            ComDataObject comDataObject = (ComDataObject)dataObject;

            var fileNames = GetFileGroupFileNames(dataObject);

            FileGroup fileGroup = new FileGroup();
            for (int i = 0; i < fileNames.Length; i++)
            {
                // Use a local index, otherwise the delegate will use the last value of i
                int localIndex = i;
                string name = fileNames[i];
                fileGroup.Items.Add(new FileGroupItem(name, (outputFile) => CopyFileGroupItem(comDataObject, localIndex, outputFile)));
            }

            return fileGroup;
        }

        public static void SetFileGroup(this IDataObject dataObject, FileGroup fileGroup)
        {
            if (fileGroup.Items.Count != 1)
            {
                throw new NotSupportedException("This method only supports file groups with a single file currently");
            }

            dataObject.SetData(CustomDataFormats.PreferredDropEffect, "DROPEFFECT_COPY");
            dataObject.SetData(CustomDataFormats.FileGroupDescriptorW, fileGroup.GetFileDescriptorStream());
            dataObject.SetData(CustomDataFormats.FileContents, fileGroup.Items[0].GetFileContentsStream());
        }

        public static void SetUri(this IDataObject dataObject, Uri uri, string description = null)
        {
            Assert.ParamIsNotNull(uri, "uri");
            Assert.ParamIs(uri.IsAbsoluteUri, "uri", "URI cannot be relative");

            string absoluteUri = uri.AbsoluteUri;

            dataObject.SetData(CustomDataFormats.UniformResourceLocator, GetMemoryStream(absoluteUri, Encoding.ASCII));
            dataObject.SetData(CustomDataFormats.UniformResourceLocatorW, GetMemoryStream(absoluteUri, Encoding.Unicode));

            bool hasRtf = dataObject.GetDataPresent(DataFormats.Rtf);

            if (!dataObject.GetDataPresent(DataFormats.UnicodeText))
            {
                dataObject.SetData(DataFormats.Text, uri.AbsoluteUri);
            }

            if (!hasRtf && description != null)
            {
                dataObject.SetData(DataFormats.Rtf, RtfUtilities.CreateRtfHyperlink(absoluteUri, description));
            }

            if(!dataObject.GetDataPresent(DataFormats.Html) && description != null)
            {
                dataObject.SetHtml($"<a href=\"{Uri.EscapeUriString(absoluteUri)}\">{WebUtility.HtmlEncode(description)}<a/>");
            }

            if (!dataObject.ContainsFileGroup())
            {
                string shortcutFormat = "[InternetShortcut]" + Environment.NewLine + "URL={0}";
                string urlFileContents = String.Format(shortcutFormat, absoluteUri);
                MemoryStream urlFileStream = GetMemoryStream(urlFileContents, Encoding.UTF8);

                string itemName = (description != null) ? description : absoluteUri;

                // Ensure it is a valid filename and it is trimmed to a maximum length
                itemName = PathUtilities.ToValidFileName(itemName);

                if (itemName.Length > 256)
                    itemName = itemName.Substring(0, 256);

                itemName = Path.ChangeExtension(itemName, ".url");

                FileGroupItem item = new FileGroupItem(itemName, () => urlFileStream);
                FileGroup fileGroup = new FileGroup();
                fileGroup.Items.Add(item);

                dataObject.SetFileGroup(fileGroup);
            }
        }

        public static void SetHtml(this IDataObject dataObject, string html)
        {
            var htmlDataString = HtmlDataFactory.GetHtmlDataString(html);
            dataObject.SetData(DataFormats.Html, htmlDataString);
        }

        private static string[] GetFileGroupFileNames(IDataObject dataObject)
        {
            // we only care about the file name, so we are skipping all the data associated with the file
            string[] filenames = new string[0];

            try
            {
                MemoryStream fileGroupDescriptorStream = (MemoryStream)dataObject.GetData(CustomDataFormats.FileGroupDescriptorW);

                using (fileGroupDescriptorStream)
                using (BinaryReader reader = new BinaryReader(fileGroupDescriptorStream, Encoding.Unicode))
                {
                    int numFiles = reader.ReadInt32();
                    filenames = new string[numFiles];

                    for (int i = 0; i < numFiles; i++)
                    {
                        reader.BaseStream.Position = 4 + i * SizeOfFileDescriptorW + OffsetToFileName;

                        char[] filenameCharArray = reader.ReadChars(260);
                        int startIndex = 0;
                        int indexOfNullCharacter = Array.IndexOf(filenameCharArray, '\0');
                        int length = (indexOfNullCharacter >= 0) ? indexOfNullCharacter : filenameCharArray.Length;
                        string filename = new String(filenameCharArray, startIndex, length); ;

                        filenames[i] = filename;
                    }
                }
            }
            catch (Exception e)
            {
                Log.WarnAndBreak(e, "Could not extract filenames from FileGroupDescriptor data object");
            }

            return filenames;
        }

        private static void CopyFileGroupItem(ComDataObject dataObject, int index, string targetFileName)
        {
            FORMATETC formatetc = new FORMATETC();
            formatetc.cfFormat = (short)DataFormats.GetDataFormat(CustomDataFormats.FileContents).Id;
            formatetc.dwAspect = DVASPECT.DVASPECT_CONTENT;
            formatetc.lindex = index;
            formatetc.ptd = IntPtr.Zero;

            // outlook uses IStorage as a medium to store the content of an email, 
            // but for data attachment it uses ISTREAM instead, so we handle both cases here
            formatetc.tymed = TYMED.TYMED_ISTORAGE | TYMED.TYMED_ISTREAM;

            STGMEDIUM medium = new STGMEDIUM();
            dataObject.GetData(ref formatetc, out medium);

            if (medium.tymed == TYMED.TYMED_ISTORAGE)
            {
                IStorage storage = null;
                IStorage storageOutput = null;

                try
                {
                    storage = (IStorage)Marshal.GetObjectForIUnknown(medium.unionmember);
                    Marshal.Release(medium.unionmember);

                    NativeMethods.StgCreateDocfile(targetFileName, (uint)(STGM.STGM_READWRITE | STGM.STGM_SHARE_EXCLUSIVE), 0, out storageOutput);

                    if (storage != null && storageOutput != null)
                    {
                        storage.CopyTo(0, null, IntPtr.Zero, storageOutput);
                        storageOutput.Commit(0);
                    }
                }
                finally
                {
                    if (storage != null) { Marshal.ReleaseComObject(storage); }
                    if (storageOutput != null) { Marshal.ReleaseComObject(storageOutput); }
                }
            }
            else
            {
                IStream stream = (IStream)Marshal.GetObjectForIUnknown(medium.unionmember);
                Marshal.Release(medium.unionmember);

                using (StreamAdapter streamAdapter = new StreamAdapter(stream))
                using (FileStream output = File.Create(targetFileName))
                {
                    streamAdapter.CopyTo(output);
                }
            }
        }

        private static MemoryStream GetMemoryStream(string text, Encoding encoding)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream, encoding);
            writer.Write(text);
            writer.Flush();

            // Write end of stream marker...
            stream.WriteByte(0);
            stream.Position = 0;
            return stream;
        }
    }
}
