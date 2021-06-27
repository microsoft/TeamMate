using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace Microsoft.Tools.TeamMate.Foundation.Native
{
    /// <summary>
    /// Basic implementation of the IStream interface (wraps a .NET Stream).
    /// </summary>
    /// <remarks>
    /// Some of the methods are not supported supported, see implementation for more details.
    /// </remarks>
    internal class ComStreamAdapter : IStream, IDisposable
    {
        private const long POSITION_NOT_SET = -1;
        private const int STG_E_INVALIDFUNCTION = 32774;
        private const int CHUNK = 4096;

        private const string METHOD_NOT_SUPPORTED = "Method not supported.";
        private const string UNKNOWN_ERROR = "Unknown error.";

        private long indexPosition = POSITION_NOT_SET;
        private Stream stream;

        /// <summary>
        /// Initializes a new instance of the StreamWrapper with the specified input stream.
        /// </summary>
        /// <param name="stream">The stream being wrapped.</param>
        internal ComStreamAdapter(Stream stream)
        {
            this.indexPosition = POSITION_NOT_SET;
            this.stream = stream;
        }

        #region IStream Members

        /// <summary>
        /// Creates a new stream object with its own seek pointer that references the same bytes as the original stream.
        /// </summary>
        /// <param name="ppstm">When this method returns, contains the new stream object. This parameter is passed uninitialized.</param>
        public void Clone(out IStream ppstm)
        {
            ppstm = null;
            ThrowNotSupportedException();
        }

        /// <summary>
        /// Ensures that any changes made to a stream object that is open in transacted mode are reflected in the parent storage.
        /// </summary>
        /// <param name="grfCommitFlags">A value that controls how the changes for the stream object are committed.</param>
        public void Commit(int grfCommitFlags)
        {
            stream.Flush();
        }

        /// <summary>
        /// Copies a specified number of bytes from the current seek pointer in the stream to the current seek pointer in another stream.
        /// </summary>
        /// <param name="pstm">A reference to the destination stream.</param>
        /// <param name="cb">The number of bytes to copy from the source stream.</param>
        /// <param name="pcbRead">On successful return, contains the actual number of bytes read from the source.</param>
        /// <param name="pcbWritten">On successful return, contains the actual number of bytes written to the destination.</param>
        public void CopyTo(IStream pstm, long cb, IntPtr pcbRead, IntPtr pcbWritten)
        {
            byte[] buffer = new byte[CHUNK];
            long written = 0;
            int read = 0;

            if (cb != 0)
            {
                SetSizeToPosition();

                do
                {
                    int count = CHUNK;
                    if (written + CHUNK > cb)
                    {
                        count = (int)(cb - written);
                    }

                    if ((read = stream.Read(buffer, 0, count)) == 0)
                    {
                        break;
                    }

                    pstm.Write(buffer, read, IntPtr.Zero);
                    written += read;

                } 
                while (written < cb);
            }

            if (pcbRead != IntPtr.Zero)
            {
                Marshal.WriteInt64(pcbRead, written);
            }

            if (pcbWritten != IntPtr.Zero)
            {
                Marshal.WriteInt64(pcbWritten, written);
            }
        }

        /// <summary>
        /// Reads a specified number of bytes from the stream object into memory starting at the current seek pointer.
        /// </summary>
        /// <param name="pv">When this method returns, contains the data read from the stream. This parameter is passed uninitialized.</param>
        /// <param name="cb">The number of bytes to read from the stream object.</param>
        /// <param name="pcbRead">A pointer to a ULONG variable that receives the actual number of bytes read from the stream object.</param>
        public void Read(byte[] pv, int cb, IntPtr pcbRead)
        {
            int read = 0;

            if (cb != 0)
            {
                SetSizeToPosition();
                read = stream.Read(pv, 0, cb);
            }

            if (pcbRead != IntPtr.Zero)
            {
                Marshal.WriteInt32(pcbRead, read);
            }
        }

        /// <summary>
        /// Changes the seek pointer to a new location relative to the beginning of the stream, to the end of the stream, or to the current seek pointer.
        /// </summary>
        /// <param name="dlibMove">The displacement to add to <paramref name="dwOrigin" />.</param>
        /// <param name="dwOrigin">The origin of the seek. The origin can be the beginning of the file, the current seek pointer, or the end of the file.</param>
        /// <param name="plibNewPosition">On successful return, contains the offset of the seek pointer from the beginning of the stream.</param>
        /// <exception cref="System.Runtime.InteropServices.ExternalException">
        /// </exception>
        public void Seek(long dlibMove, int dwOrigin, IntPtr plibNewPosition)
        {
            long newPosition = 0;
            SeekOrigin seekOrigin = SeekOrigin.Begin;

            try
            {
                seekOrigin = (SeekOrigin)dwOrigin;
            }
            catch
            {
                throw new ExternalException(UNKNOWN_ERROR, STG_E_INVALIDFUNCTION);
            }

            if (stream.CanWrite)
            {
                switch (seekOrigin)
                {
                    case SeekOrigin.Begin:
                        newPosition = dlibMove;
                        break;

                    case SeekOrigin.Current:
                        newPosition = indexPosition;
                        if (newPosition == POSITION_NOT_SET)
                        {
                            newPosition = stream.Position;
                        }

                        newPosition += dlibMove;
                        break;

                    case SeekOrigin.End:
                        newPosition = stream.Length + dlibMove;
                        break;

                    default:
                        // should never happen
                        throw new ExternalException(UNKNOWN_ERROR, STG_E_INVALIDFUNCTION);
                }

                if (newPosition > stream.Length)
                {
                    indexPosition = newPosition;
                }
                else
                {
                    stream.Position = newPosition;
                    indexPosition = POSITION_NOT_SET;
                }
            }
            else
            {
                try
                {
                    newPosition = stream.Seek(dlibMove, seekOrigin);
                }
                catch (ArgumentException)
                {
                    throw new ExternalException(UNKNOWN_ERROR, STG_E_INVALIDFUNCTION);
                }

                indexPosition = POSITION_NOT_SET;
            }

            if (plibNewPosition != IntPtr.Zero)
            {
                Marshal.WriteInt64(plibNewPosition, newPosition);
            }
        }

        /// <summary>
        /// Changes the size of the stream object.
        /// </summary>
        /// <param name="libNewSize">The new size of the stream as a number of bytes.</param>
        public void SetSize(long libNewSize)
        {
            stream.SetLength(libNewSize);
        }

        /// <summary>
        /// Retrieves the <see cref="T:System.Runtime.InteropServices.STATSTG" /> structure for this stream.
        /// </summary>
        /// <param name="pstatstg">When this method returns, contains a STATSTG structure that describes this stream object. This parameter is passed uninitialized.</param>
        /// <param name="grfStatFlag">Members in the STATSTG structure that this method does not return, thus saving some memory allocation operations.</param>
        public void Stat(out System.Runtime.InteropServices.ComTypes.STATSTG pstatstg, int grfStatFlag)
        {
            pstatstg = new System.Runtime.InteropServices.ComTypes.STATSTG();
            pstatstg.cbSize = stream.Length;
        }

        /// <summary>
        /// Writes a specified number of bytes into the stream object starting at the current seek pointer.
        /// </summary>
        /// <param name="pv">The buffer to write this stream to.</param>
        /// <param name="cb">The number of bytes to write to the stream.</param>
        /// <param name="pcbWritten">On successful return, contains the actual number of bytes written to the stream object. If the caller sets this pointer to <see cref="F:System.IntPtr.Zero" />, this method does not provide the actual number of bytes written.</param>
        public void Write(byte[] pv, int cb, IntPtr pcbWritten)
        {
            if (cb != 0)
            {
                SetSizeToPosition();
                stream.Write(pv, 0, cb);
            }

            if (pcbWritten != IntPtr.Zero)
            {
                Marshal.WriteInt32(pcbWritten, cb);
            }
        }

        /// <summary>
        /// Discards all changes that have been made to a transacted stream since the last <see cref="M:System.Runtime.InteropServices.ComTypes.IStream.Commit(System.Int32)" /> call.
        /// </summary>
        /// <exception cref="System.Runtime.InteropServices.ExternalException"></exception>
        public void Revert()
        {
            throw new ExternalException(METHOD_NOT_SUPPORTED, STG_E_INVALIDFUNCTION);
        }

        /// <summary>
        /// Restricts access to a specified range of bytes in the stream.
        /// </summary>
        /// <param name="libOffset">The byte offset for the beginning of the range.</param>
        /// <param name="cb">The length of the range, in bytes, to restrict.</param>
        /// <param name="dwLockType">The requested restrictions on accessing the range.</param>
        public void LockRegion(long libOffset, long cb, int dwLockType)
        {
            ThrowNotSupportedException();
        }

        /// <summary>
        /// Removes the access restriction on a range of bytes previously restricted with the <see cref="M:System.Runtime.InteropServices.ComTypes.IStream.LockRegion(System.Int64,System.Int64,System.Int32)" /> method.
        /// </summary>
        /// <param name="libOffset">The byte offset for the beginning of the range.</param>
        /// <param name="cb">The length, in bytes, of the range to restrict.</param>
        /// <param name="dwLockType">The access restrictions previously placed on the range.</param>
        public void UnlockRegion(long libOffset, long cb, int dwLockType)
        {
            ThrowNotSupportedException();
        }

        #endregion

        /// <summary>
        /// Throws a not supported exception.
        /// </summary>
        private void ThrowNotSupportedException()
        {
            throw new ExternalException(METHOD_NOT_SUPPORTED, STG_E_INVALIDFUNCTION);
        }

        /// <summary>
        /// Sets the stream length to the current index position.
        /// </summary>
        private void SetSizeToPosition()
        {
            if (indexPosition != POSITION_NOT_SET)
            {
                // position requested greater than current length?
                if (indexPosition > stream.Length)
                {
                    // expand stream
                    stream.SetLength(indexPosition);
                }

                // set new position
                stream.Position = indexPosition;
                indexPosition = POSITION_NOT_SET;
            }
        }

        /// <summary>
        /// Closes this instance.
        /// </summary>
        public void Close()
        {
            stream.Close();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            stream.Dispose();
        }
    }
}
