using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.IO
{
    /// <summary>
    /// A class to manage the creation and disposal of temporary directories.
    /// </summary>
    public class TempDirectory : IDisposable
    {
        private string path;
        private bool disposed;

        /// <summary>
        /// Creates a temporary directory for the current process (based on the process name).
        /// </summary>
        /// <returns>The temporary directory.</returns>
        public static TempDirectory CreateForProcess()
        {
            Process process = Process.GetCurrentProcess();
            string folderName = String.Format("{0}.{1}", System.IO.Path.GetFileNameWithoutExtension(process.ProcessName), process.Id);
            folderName = PathUtilities.ToValidFileName(folderName);
            string path = PathUtilities.GetUniqueOrRandomFilename(System.IO.Path.GetTempPath(), folderName);
            return new TempDirectory(path);
        }

        /// <summary>
        /// Creates a temporary directory in the TEMP directory, with an optional preferred name.
        /// </summary>
        /// <param name="preferredName">An optional preferred name.</param>
        /// <returns>The temporary directory.</returns>
        public static TempDirectory CreateInTempPath(string preferredName = null)
        {
            string path = PathUtilities.GetTempFilename(preferredName);
            return new TempDirectory(path);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TempDirectory"/> class.
        /// </summary>
        /// <param name="path">The path of the temporary directory.</param>
        /// <param name="createImmediately">if set to <c>true</c>, creates the directory immediately. Otherwise, the directory is created later on demand.</param>
        public TempDirectory(string path, bool createImmediately = true)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            this.path = path;
            if (createImmediately)
            {
                EnsureIsCreated();
            }
        }

        /// <summary>
        /// Creates a temporary sub directory.
        /// </summary>
        /// <param name="preferredName">An optional preferred name..</param>
        /// <param name="createImmediately">if set to <c>true</c>, creates the directory immediately. Otherwise, the directory is created later on demand.</param>
        /// <returns></returns>
        public TempDirectory CreateTempSubDirectory(string preferredName = null, bool createImmediately = true)
        {
            return new TempDirectory(GetUniqueOrRandomFilename(preferredName), createImmediately);
        }

        /// <summary>
        /// Ensures that the temporary directory has been created (used when the instance was created with createImmediately = false).
        /// </summary>
        public void EnsureIsCreated()
        {
            PathUtilities.EnsureDirectoryExists(this.path);
        }

        /// <summary>
        /// Gets the path to the temp directory.
        /// </summary>
        public string Path
        {
            get { return this.path; }
        }

        /// <summary>
        /// Gets a unique or random filename in the temp directory.
        /// </summary>
        /// <param name="preferredName">An optional preferred name.</param>
        /// <returns>The file path.</returns>
        public string GetUniqueOrRandomFilename(string preferredName = null)
        {
            return PathUtilities.GetUniqueOrRandomFilename(this.path, preferredName);
        }

        /// <summary>
        /// Deletes the directory and all of its contents.
        /// </summary>
        public void Delete()
        {
            if (Directory.Exists(this.path))
            {
                PathUtilities.DeleteRecursively(this.path, DeleteMode.Force);
            }
        }

        /// <summary>
        /// Deletes the directory contents..
        /// </summary>
        public void DeleteContents()
        {
            PathUtilities.DeleteContents(this.path, DeleteMode.Force);
        }

        /// <summary>
        /// Attempts to delete the directory and all of its contents.
        /// </summary>
        /// <returns><c>true</c> if the directory was succesfully deleted.</returns>
        public bool TryDelete()
        {
            return PathUtilities.TryDelete(this.path);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.path;
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
