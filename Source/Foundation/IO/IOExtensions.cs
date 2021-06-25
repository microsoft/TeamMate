using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.IO
{
    /// <summary>
    /// Provides extension methods for the IO package.
    /// </summary>
    public static class IOExtensions
    {
        // Copied from .NET. 80K.
        private const int DefaultCopyBufferSize = 0x14000;

        /// <summary>
        /// Copies a stream asynchronously, optionally reporting percentage progress based on the stream bytes.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="destination">The destination stream.</param>
        /// <param name="progress">The progress reporter.</param>
        /// <param name="length">The optional well known stream length.</param>
        public static async Task CopyToAsync(this Stream source, Stream destination, IProgress<double> progress, long? length)
        {
            Assert.ParamIsNotNull(source, "from");
            Assert.ParamIsNotNull(destination, "destination");

            if (length != null && length.Value <= 0)
            {
                throw new ArgumentException("Length must be greater than 0");
            }

            if (progress != null)
            {
                progress.Report(0);
            }

            long totalBytesCopied = 0;
            int byteCount;
            byte[] buffer = new byte[DefaultCopyBufferSize];
            while ((byteCount = await source.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await destination.WriteAsync(buffer, 0, byteCount);
                totalBytesCopied += byteCount;
                if (length != null && progress != null)
                {
                    double calculatedProgress = (double)totalBytesCopied / (double)length.Value;

                    // In case length was not a trustable number, never make progress greater than 100%
                    calculatedProgress = Math.Min(calculatedProgress, 1.0);

                    progress.Report(calculatedProgress);
                }
            }

            if (progress != null)
            {
                progress.Report(1.0);
            }
        }
    }
}
