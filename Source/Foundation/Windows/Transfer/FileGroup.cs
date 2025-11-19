using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Foundation.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Transfer
{
    public class FileGroup
    {
        private IList<FileGroupItem> items = new List<FileGroupItem>();

        public IList<FileGroupItem> Items
        {
            get { return this.items; }
        }

        internal MemoryStream GetFileDescriptorStream()
        {
            MemoryStream stream = new MemoryStream();

            // Write out the FILEGROUPDESCRIPTOR.cItems value
            stream.Write(BitConverter.GetBytes(Items.Count), 0, sizeof(uint));

            foreach (FileGroupItem item in Items)
            {
                FILEDESCRIPTOR fileDescriptor = new FILEDESCRIPTOR();
                fileDescriptor.cFileName = item.FileName;

                uint flags = 0;

                if (item.WriteTime != null)
                {
                    Int64 fileWriteTimeUtc = item.WriteTime.Value.ToFileTimeUtc();
                    fileDescriptor.ftLastWriteTime.dwHighDateTime = (uint)(fileWriteTimeUtc >> 32);
                    fileDescriptor.ftLastWriteTime.dwLowDateTime = (uint)(fileWriteTimeUtc & 0xFFFFFFFF);
                    flags |= (uint)FileDescriptorFlags.FD_WRITESTIME;
                }

                if (item.FileSize != null)
                {
                    fileDescriptor.nFileSizeHigh = (uint)(item.FileSize.Value >> 32);
                    fileDescriptor.nFileSizeLow = (uint)(item.FileSize.Value & 0xFFFFFFFF);
                    flags |= (uint)(FileDescriptorFlags.FD_FILESIZE | FileDescriptorFlags.FD_PROGRESSUI);
                }

                fileDescriptor.dwFlags = flags;

                // Marshal the FileDescriptor structure into a byte array and write it to the MemoryStream.
                int size = Marshal.SizeOf(fileDescriptor);
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(fileDescriptor, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                Marshal.FreeHGlobal(ptr);
                stream.Write(bytes, 0, bytes.Length);
            }

            return stream;
        }
    }

    public class FileGroupItem
    {
        private CopyToDelegate copyTo;
        private OpenStreamDelegate openStream;

        public FileGroupItem(string filename, OpenStreamDelegate openStream)
        {
            Assert.ParamIsNotNullOrEmpty(filename, "filename");
            Assert.ParamIsNotNull(openStream, "openStream");

            this.FileName = PathUtilities.ToValidFileName(filename);
            this.openStream = openStream;
        }

        internal FileGroupItem(string filename, CopyToDelegate copyTo)
        {
            Assert.ParamIsNotNullOrEmpty(filename, "filename");
            Assert.ParamIsNotNull(copyTo, "copyTo");

            this.FileName = filename;
            this.copyTo = copyTo;
        }

        public string FileName { get; private set; }
        public DateTime? WriteTime { get; set; }
        public long? FileSize { get; set; }

        public Stream OpenStream()
        {
            if (this.openStream == null)
            {
                throw new NotSupportedException();
            }

            return this.openStream();
        }

        public void CopyTo(string filename)
        {
            Assert.ParamIsNotNull(filename, "filename");

            if (this.copyTo != null)
            {
                this.copyTo(filename);
            }
            else if (this.openStream != null)
            {
                using (Stream stream = this.openStream())
                using (Stream output = File.Create(filename))
                {
                    stream.CopyTo(output);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        internal MemoryStream GetFileContentsStream()
        {
            // Get the virtual file contents and place the contents in the byte array bBuffer.
            // If the contents are zero length then a single byte must be supplied to Windows
            // Explorer otherwise the transfer will fail.  If this is part of a multi-file transfer,
            // the entire transfer will fail at this point if the buffer is zero length.
            MemoryStream stream = (FileSize != null) ? new MemoryStream((int)FileSize.Value) : new MemoryStream();

            byte[] buffer = new byte[0x14000];
            using (BinaryReader reader = new BinaryReader(OpenStream()))
            {
                int read;
                while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    stream.Write(buffer, 0, read);
                }
            }

            if (stream.Length == 0)
            {
                // Must send at least one byte for a zero length file to prevent stoppages
                buffer = new byte[1];
                stream.Write(buffer, 0, buffer.Length);
            }
            return stream;
        }
    }

    public delegate Stream OpenStreamDelegate();
    internal delegate void CopyToDelegate(string filename);
}
