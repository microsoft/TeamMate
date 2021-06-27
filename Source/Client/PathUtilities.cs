using System;
using System.IO;

namespace Microsoft.Tools.TeamMate.Client
{
    /// <summary>
    /// Provides utility methods for manipulating file system paths.
    /// </summary>
    /// <remarks>
    /// This is a trimmed local copy of Foundation.IO.PathUtilities, as we do not want dependencies
    /// to another assembly in this client one.
    /// </remarks>
    internal static class PathUtilities
    {
        /// <summary>
        /// Returns a unique filename that doesn't exist in the given path,
        /// by appending an incremented number to the filename (without the
        /// extension).
        /// </summary>
        /// <param name="path">An initial path.</param>
        /// <returns>A unique filename path derived from the input.</returns>
        public static string EnsureFilenameIsUnique(string path)
        {
            if (!Exists(path))
            {
                return path;
            }

            string name = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            string dir = Path.GetDirectoryName(path);
            int increment = 1;

            do
            {
                increment++;
                string newName = String.Format("{0} ({1}){2}", name, increment, extension);

                // TODO: Need to make sure path did not exceed max path length.
                path = Path.Combine(dir, newName);
            }
            while (Exists(path));

            return path;
        }

        /// <summary>
        /// Determines whether the specified path exists (as a file or directory).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the path exists; <c>false</c> if otherwise.</returns>
        private static bool Exists(string path)
        {
            return File.Exists(path) || Directory.Exists(path);
        }
    }
}
