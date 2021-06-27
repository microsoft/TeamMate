using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.IO;

namespace Microsoft.Tools.TeamMate.Foundation.IO
{
    /// <summary>
    /// Manages the creation and deletion of temporary files.
    /// </summary>
    public class TempFile : IDisposable
    {
        private string path;
        private bool disposed;

        /// <summary>
        /// Creates a temporary file in the TEMP folder, with an optional preferred name.
        /// </summary>
        /// <param name="preferredName">An optional preferred name.</param>
        /// <returns>The temporary file.</returns>
        public static TempFile Create(string preferredName = null)
        {
            string path = PathUtilities.GetTempFilename(preferredName);
            return new TempFile(path);
        }

        internal TempFile(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            // Make sure file exists
            PathUtilities.EnsureFileExists(path);
            this.path = path;
        }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        public string Path
        {
            get { return this.path; }
        }

        /// <summary>
        /// Deletes temporary file if it exists.
        /// </summary>
        public void Delete()
        {
            if (File.Exists(this.path))
            {
                File.Delete(this.path);
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (!this.disposed)
            {
                Delete();
                this.disposed = true;
            }
        }

        #endregion
    }
}
