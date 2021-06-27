// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using Marshal = System.Runtime.InteropServices.Marshal;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Adapts a COM IStream as a .NET Stream.
    /// </summary>
    internal class StreamAdapter : System.IO.Stream
    {
        private IStream source;

        private IntPtr ptrBytesRead;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamAdapter"/> class.
        /// </summary>
        /// <param name="source">The source COM stream.</param>
        public StreamAdapter(IStream source)
        {
            Assert.ParamIsNotNull(source, "source");

            this.source = source;
            this.ptrBytesRead = Marshal.AllocHGlobal(sizeof(uint));
        }

        ~StreamAdapter()
        {
            Dispose(false);
        }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports reading.
        /// </summary>
        /// <returns>true if the stream supports reading; otherwise, false.</returns>
        public override bool CanRead { get { return true; } }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports seeking.
        /// </summary>
        /// <returns>true if the stream supports seeking; otherwise, false.</returns>
        public override bool CanSeek { get { return true; } }

        /// <summary>
        /// When overridden in a derived class, gets a value indicating whether the current stream supports writing.
        /// </summary>
        /// <returns>true if the stream supports writing; otherwise, false.</returns>
        public override bool CanWrite { get { return true; } }

        /// <summary>
        /// When overridden in a derived class, clears all buffers for this stream and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush()
        {
            CheckDisposed();
            source.Commit(0);
        }

        /// <summary>
        /// When overridden in a derived class, gets the length in bytes of the stream.
        /// </summary>
        /// <returns>A long value representing the length of the stream in bytes.</returns>
        public override long Length
        {
            get
            {
                CheckDisposed();
                STATSTG stat;
                source.Stat(out stat, 1);
                return stat.cbSize;
            }
        }

        /// <summary>
        /// When overridden in a derived class, gets or sets the position within the current stream.
        /// </summary>
        /// <returns>The current position within the stream.</returns>
        public override long Position
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within the stream by the number of bytes read.
        /// </summary>
        /// <param name="buffer">An array of bytes. When this method returns, the buffer contains the specified byte array with the values between <paramref name="offset" /> and (<paramref name="offset" /> + <paramref name="count" /> - 1) replaced by the bytes read from the current source.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the current stream.</param>
        /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
        /// <returns>
        /// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes are not currently available, or zero (0) if the end of the stream has been reached.
        /// </returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            Assert.ParamIsNotNull(buffer, "buffer");
            Assert.ParamIsNotNegative(offset, "offset");
            Assert.ParamIsNotNegative(count, "count");

            if (offset != 0)
            {
                throw new NotImplementedException();
            }

            CheckDisposed();

            source.Read(buffer, count, ptrBytesRead);
            return Marshal.ReadInt32(ptrBytesRead);
        }

        /// <summary>
        /// When overridden in a derived class, sets the position within the current stream.
        /// </summary>
        /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
        /// <param name="origin">A value of type <see cref="T:System.IO.SeekOrigin" /> indicating the reference point used to obtain the new position.</param>
        /// <returns>
        /// The new position within the current stream.
        /// </returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            Assert.ParamIsNotNegative(offset, "offset");

            CheckDisposed();
            
            source.Seek(offset, (int)origin, ptrBytesRead);
            return Marshal.ReadInt64(ptrBytesRead);
        }

        /// <summary>
        /// When overridden in a derived class, sets the length of the current stream.
        /// </summary>
        /// <param name="value">The desired length of the current stream in bytes.</param>
        public override void SetLength(long value)
        {
            Assert.ParamIsNotNegative(value, "value");
            CheckDisposed();

            source.SetSize(value);
        }

        /// <summary>
        /// When overridden in a derived class, writes a sequence of bytes to the current stream and advances the current position within this stream by the number of bytes written.
        /// </summary>
        /// <param name="buffer">An array of bytes. This method copies <paramref name="count" /> bytes from <paramref name="buffer" /> to the current stream.</param>
        /// <param name="offset">The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current stream.</param>
        /// <param name="count">The number of bytes to be written to the current stream.</param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Assert.ParamIsNotNull(buffer, "buffer");
            Assert.ParamIsNotNegative(offset, "offset");
            Assert.ParamIsNotNegative(count, "count");

            if (offset != 0)
            {
                throw new NotImplementedException();
            }

            CheckDisposed();

            source.Write(buffer, count, IntPtr.Zero);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (!this.disposed)
            {
                if (ptrBytesRead != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptrBytesRead);
                    ptrBytesRead = IntPtr.Zero;
                }

                if (source != null)
                {
                    Marshal.ReleaseComObject(source);
                    source = null;
                }

                this.disposed = true;
            }
        }

        /// <summary>
        /// Checks that an item has not beend dispose before attempting to use it.
        /// </summary>
        private void CheckDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
