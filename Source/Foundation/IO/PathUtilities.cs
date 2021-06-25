using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.IO
{
    /// <summary>
    /// Provides utility methods for manipulating file system paths.
    /// </summary>
    public static class PathUtilities
    {
        /// <summary>
        /// A fully qualified file name length must be less or equal to this value.
        /// </summary>
        public const int MaxPathLength = 260 - 1;   // Remove one character for NULL, this is a legacy string constant from C

        /// <summary>
        /// A directory name must be less than or equal to this value.
        /// </summary>
        public const int MaxDirectoryLength = 248 - 1;  // Remove one character for NULL, this is a legacy string constant from C

        /// <summary>
        /// Checks if two filenames are equal.
        /// </summary>
        /// <param name="filename1">A filename.</param>
        /// <param name="filename2">Another filename.</param>
        /// <returns><c>true</c> if the filenames are equal.</returns>
        public static bool FileNamesAreEqual(string filename1, string filename2)
        {
            return String.Equals(filename1, filename2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if two paths are equal.
        /// </summary>
        /// <param name="path1">A path.</param>
        /// <param name="path2">Another path.</param>
        /// <returns><c>true</c> if the paths are equal.</returns>
        public static bool PathsAreEqual(string path1, string path2)
        {
            return String.Equals(path1, path2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if two file extensions are equal.
        /// </summary>
        /// <param name="extension1">A file extension.</param>
        /// <param name="extension2">Another file extension.</param>
        /// <returns><c>true</c> if the extensions are equal.</returns>
        public static bool ExtensionsAreEqual(string extension1, string extension2)
        {
            return String.Equals(extension1, extension2, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Attempts to find a file in the PATH environment variable.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>The full path to the found file, or <c>null</c> if it was not found in any of the folders
        /// in the PATH environment variable.</returns>
        public static string FindInPath(string filename)
        {
            string path = Environment.GetEnvironmentVariable("PATH");
            if (path != null)
            {
                var pathTokens = path.Split(Path.PathSeparator).Select(p => p.Trim()).Where(p => !String.IsNullOrEmpty(p));
                foreach (string pathEntry in pathTokens)
                {
                    string fullPath = Path.Combine(pathEntry, filename);
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }

            // Not found
            return null;
        }


        /// <summary>
        /// Converts an arbitrary string name to a valid filename by trimming or replacing invalid characters.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="replacement">An optional replacement character (none by default, invalid characters will be removed).</param>
        /// <returns>A valid filename to use for saving to a file.</returns>
        public static string ToValidFileName(string name, char replacement = (char) 0)
        {
            Assert.ParamIsNotNull(name, "name");

            var invalidChars = Path.GetInvalidFileNameChars();
            StringBuilder sb = new StringBuilder(name);
            for (int i = 0; i < sb.Length; i++)
            {
                if (invalidChars.Contains(sb[i]))
                {
                    sb.Remove(i, 1);

                    if (replacement > 0)
                    {
                        sb.Insert(i, replacement);
                    }
                    else
                    {
                        i--;
                    }
                }
            }

            if (name.Length > 0 && sb.Length == 0)
            {
                // TODO: All characters were trimmed, how do we make this a valid filename? Arbitrarily assign some value?
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a unique temporary filename in the current user's temporary folder.
        /// </summary>
        /// <param name="preferredName">An otional preferred name for the file (<c>null</c> by default, which generates a random name).</param>
        /// <returns>The unique or random filename under the current user's temporary folder.</returns>
        public static string GetTempFilename(string preferredName = null)
        {
            return GetUniqueOrRandomFilename(Path.GetTempPath(), preferredName);
        }

        /// <summary>
        /// Gets a unique or random filename under a given parent path.
        /// </summary>
        /// <param name="parentPath">The parent path.</param>
        /// <param name="preferredName">An otional preferred name for the file (<c>null</c> by default, which generates a random name).</param>
        /// <returns>The unique or random filename under a given directory.</returns>
        public static string GetUniqueOrRandomFilename(string parentPath, string preferredName = null)
        {
            Assert.ParamIsNotNullOrEmpty(parentPath, "parentPath");

            if (preferredName == null)
            {
                preferredName = Path.GetRandomFileName();
            }

            string path = Path.Combine(parentPath, preferredName);

            // Use a padding in case the name is made unique further down, where e.g. " (17)" is appended (giving ourselves a 5 char buffer)
            int additionalPadding = 5;

            int excessLength = (path.Length + additionalPadding) - MaxPathLength;
            if (excessLength > 0)
            {
                bool trimSucceeded = false;
                string baseName = Path.GetFileNameWithoutExtension(preferredName);
                if (baseName.Length > excessLength)
                {
                    // We can trim the base filename (keeping the extension as is) to a safer full path length
                    baseName = baseName.Substring(0, baseName.Length - excessLength).TrimEnd();
                    if (baseName.Length > 0)
                    {
                        string extension = Path.GetExtension(preferredName);
                        path = Path.Combine(parentPath, baseName + extension);
                        trimSucceeded = true;
                    }
                }

                if (!trimSucceeded)
                {
                    Log.WarnAndBreak("Could not trim path length to fit within valid length, directory name was too long: {0}", path);
                }
            }

            path = EnsureFilenameIsUnique(path);

            return path;
        }

        /// <summary>
        /// Returns a unique filename that doesn't exist in the given path,
        /// by appending an incremented number to the filename (without the
        /// extension).
        /// </summary>
        /// <param name="path">An initial path.</param>
        /// <returns>A unique filename path derived from the input.</returns>
        public static string EnsureFilenameIsUnique(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

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
        public static bool Exists(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            return File.Exists(path) || Directory.Exists(path);
        }

        /// <summary>
        /// Tries to delete a file or folder.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The deletion mode.</param>
        /// <returns><c>true</c> if th edeletion was successfull or there was nothign to delete (the file didn't exist). <c>false</c> if the deletion failed.</returns>
        public static bool TryDelete(string path, DeleteMode mode = DeleteMode.None)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                else if (Directory.Exists(path))
                {
                    DeleteRecursively(path, mode);
                }

                return true;
            }
            catch (Exception e)
            {
                Log.ErrorAndBreak(e, "Error attempting to delete file or folder with path: " + path);
            }

            return false;
        }

        /// <summary>
        /// Deletes a directory recursively.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The deletion mode.</param>
        public static void DeleteRecursively(string path, DeleteMode mode = DeleteMode.None)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            DeleteRecursively(new DirectoryInfo(path), mode);
        }

        /// <summary>
        /// Recursively deletes a directory.
        /// </summary>
        /// <param name="directory">The base directory.</param>
        public static void DeleteRecursively(DirectoryInfo directory, DeleteMode mode = DeleteMode.None)
        {
            Assert.ParamIsNotNull(directory, "directory");

            DeleteContents(directory, mode);

            bool force = (mode & DeleteMode.Force) == DeleteMode.Force;

            try
            {
                if (force)
                {
                    directory.Attributes &= ~FileAttributes.ReadOnly;
                }

                directory.Delete();
            }
            catch (IOException e)
            {
                throw new IOException("Couldn't delete " + directory.FullName, e);
            }
        }

        /// <summary>
        /// Deletes the contents of a given directory (but not the directory itself).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The deletion mode.</param>
        public static void DeleteContents(string path, DeleteMode mode = DeleteMode.None)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            DeleteContents(new DirectoryInfo(path), mode);
        }

        /// <summary>
        /// Deletes the contents of a given directory (but not the directory itself).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="mode">The deletion mode.</param>
        public static void DeleteContents(DirectoryInfo directory, DeleteMode mode = DeleteMode.None)
        {
            Assert.ParamIsNotNull(directory, "directory");


            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                DeleteRecursively(subDirectory, mode);

            bool force = (mode & DeleteMode.Force) == DeleteMode.Force;

            foreach (FileInfo file in directory.GetFiles())
            {
                try
                {
                    if (force)
                    {
                        file.Attributes &= ~FileAttributes.ReadOnly;
                    }

                    file.Delete();
                }
                catch (IOException e)
                {
                    throw new IOException("Couldn't delete " + file.FullName, e);
                }
            }
        }

        /// <summary>
        /// Copies a directory recursively.
        /// </summary>
        /// <param name="sourceDirectory">The source directory.</param>
        /// <param name="targetDirectory">The target directory.</param>
        public static void CopyRecursively(string sourceDirectory, string targetDirectory)
        {
            Assert.ParamIsNotNullOrEmpty(sourceDirectory, "sourceDirectory");
            Assert.ParamIsNotNullOrEmpty(targetDirectory, "targetDirectory");

            if (!Directory.Exists(sourceDirectory))
            { 
                throw new DirectoryNotFoundException("Invalid source directory " + sourceDirectory);
            }

            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            string[] sources = Directory.GetFileSystemEntries(sourceDirectory);
            foreach (string source in sources)
            {
                string target = Path.Combine(targetDirectory, Path.GetFileName(source));
                if (File.Exists(source))
                {
                    File.Copy(source, target, true);
                }
                else if (Directory.Exists(source))
                {
                    CopyRecursively(source, target);
                }
            }
        }

        /// <summary>
        /// Ensures the given directory exists, and creates it if it doesn't.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void EnsureDirectoryExists(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Ensures the parent directory of the given path exists, and creates it if it doesn't.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void EnsureParentDirectoryExists(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            EnsureDirectoryExists(Path.GetDirectoryName(path));
        }

        /// <summary>
        /// Ensures that a file exists, or creates a 0-byte file if it doesn't.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void EnsureFileExists(string path)
        {
            Assert.ParamIsNotNullOrEmpty(path, "path");

            if (!File.Exists(path))
            {
                EnsureParentDirectoryExists(path);
                File.WriteAllBytes(path, new byte[0]);
            }
        }

        /// <summary>
        /// Ensures that a file path is writeable. If the file exists and is read-only,
        /// it will be made writeable. Otherwise, we ensure that its parent directory
        /// exists.
        /// </summary>
        /// <param name="filename">The filename that will be written to.</param>
        public static void EnsureFileIsWriteable(string filename)
        {
            Assert.ParamIsNotNullOrEmpty(filename, "filename");


            if (File.Exists(filename))
            {
                // Ensure the file is not read-only
                FileAttributes attributes = File.GetAttributes(filename);
                if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    attributes &= ~FileAttributes.ReadOnly;
                    File.SetAttributes(filename, attributes);
                }
            }
            else
            {
                EnsureParentDirectoryExists(filename);
            }
        }

        /// <summary>
        /// Returns the path of the given full path relative to a base path.
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="fullPath">The full path.</param>
        /// <returns>The path of the given full path relative to a base path.</returns>
        public static string GetRelativePath(string basePath, string fullPath)
        {
            Assert.ParamIsNotNullOrEmpty(basePath, "basePath");
            Assert.ParamIsNotNullOrEmpty(fullPath, "fullPath");

            if (!Path.IsPathRooted(basePath))
            {
                throw new ArgumentException("Base path " + basePath + " is not rooted");
            }

            if (!Path.IsPathRooted(fullPath))
            {
                throw new ArgumentException("Full path " + fullPath + " is not rooted");
            }

            if (!fullPath.StartsWith(basePath, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Full path " + fullPath + " is not a descendant of " + basePath);
            }

            return fullPath.Substring(basePath.Length).TrimStart(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Finds a set of files and directories matching the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the names of files.
        /// Can include the following wildcards: ..., *, ?.</param>
        /// <param name="baseDirectory">The base directory to start the search from (Optional). If
        /// the search pattern defines a root path, this parameter is ignored. If <c>null</c>,
        /// then the current directory will be used.</param>
        /// <returns>
        /// A collection of the files and directories matching the search pattern.
        /// </returns>
        /// <remarks>
        /// 	<para>The following wildcards are permitted in the search pattern:</para>
        /// 	<list>
        /// 		<item>*: Matches zero or more characters</item>
        /// 		<item>?: Exactly one character</item>
        /// 		<item>...: Search in all subdirectories</item>
        /// 	</list>
        /// 	<para>If the search pattern does not define a root path, then the current directory
        /// will be the starting point of the search.</para>
        /// </remarks>
        public static ICollection<string> FindFileSystemEntries(string searchPattern, string baseDirectory = null)
        {
            Assert.ParamIsNotNullOrEmpty(searchPattern, "searchPattern");

            return Find(searchPattern, SearchType.FilesAndDirectories, baseDirectory);
        }

        /// <summary>
        /// Finds a set of directories matching the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the names of directories.
        /// Can include the following wildcards: ..., *, ?.</param>
        /// <param name="baseDirectory">The base directory to start the search from (Optional). If
        /// the search pattern defines a root path, this parameter is ignored. If <c>null</c>,
        /// then the current directory will be used.</param>
        /// <returns>A collection of the directories matching the search pattern.</returns>
        /// <remarks>
        /// <para>The following wildcards are permitted in the search pattern:</para>
        /// <list>
        ///   <item>*: Matches zero or more characters</item>
        ///   <item>?: Exactly one character</item>
        ///   <item>...: Search in all subdirectories</item>
        /// </list>
        /// <para>If the search pattern does not define a root path, then the current directory
        /// will be the starting point of the search.</para>
        /// </remarks>
        public static ICollection<string> FindDirectories(string searchPattern, string baseDirectory = null)
        {
            Assert.ParamIsNotNullOrEmpty(searchPattern, "searchPattern");

            return Find(searchPattern, SearchType.Directories, baseDirectory);
        }

        /// <summary>
        /// Finds a set of files matching the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the names of files.
        /// Can include the following wildcards: ..., *, ?.</param>
        /// <param name="baseDirectory">The base directory to start the search from (Optional). If
        /// the search pattern defines a root path, this parameter is ignored. If <c>null</c>,
        /// then the current directory will be used.</param>
        /// <returns>A collection of the files matching the search pattern.</returns>
        /// <remarks>
        /// <para>The following wildcards are permitted in the search pattern:</para>
        /// <list>
        ///   <item>*: Matches zero or more characters</item>
        ///   <item>?: Exactly one character</item>
        ///   <item>...: Search in all subdirectories</item>
        /// </list>
        /// <para>If the search pattern does not define a root path, then the current directory
        /// will be the starting point of the search.</para>
        /// </remarks>
        public static ICollection<string> FindFiles(string searchPattern, string baseDirectory = null)
        {
            Assert.ParamIsNotNullOrEmpty(searchPattern, "searchPattern");

            return Find(searchPattern, SearchType.Files, baseDirectory);
        }

        /// <summary>
        /// Finds a set of files and/or directories matching the specified search pattern.
        /// </summary>
        /// <param name="searchPattern">The search string to match against the names of files.
        /// Can include the following wildcards: ..., *, ?.</param>
        /// <param name="searchType">Defines the type of file system entry being searched for.</param>
        /// <param name="baseDirectory">The base directory to start the search from (Optional). If
        /// the search pattern defines a root path, this parameter is ignored. If <c>null</c>,
        /// then the current directory will be used.</param>
        /// <returns>
        /// A collection of the files and directories matching the search pattern.
        /// </returns>
        /// <remarks>
        /// 	<para>The following wildcards are permitted in the search pattern:</para>
        /// 	<list>
        /// 		<item>*: Matches zero or more characters</item>
        /// 		<item>?: Exactly one character</item>
        /// 		<item>...: Search in all subdirectories</item>
        /// 	</list>
        /// 	<para>If the search pattern does not define a root path, then the current directory
        /// will be the starting point of the search.</para>
        /// </remarks>
        private static ICollection<string> Find(string searchPattern, SearchType searchType, string baseDirectory)
        {
            // First expand any environment variable definition in the search pattern.
            searchPattern = Environment.ExpandEnvironmentVariables(searchPattern);

            if (baseDirectory == null)
            {
                baseDirectory = Environment.CurrentDirectory;
            }

            if (!ContainsWildcards(searchPattern))
            {
                IList<string> result = new List<string>();

                // No wildcards, optimized to search for an absolut or relative path
                string fullPath = Path.Combine(baseDirectory, searchPattern);
                if (PathMatches(fullPath, searchType))
                {
                    result.Add(Path.GetFullPath(fullPath));
                }

                return result;
            }
            else
            {
                // The pattern contains wildcards, tokenize and resolve each level...
                ICollection<string> currentLevel = new List<string>();
                ICollection<string> nextLevel = new List<string>();

                // Figure out if the search pattern defines a root, otherwise we need to use the
                // base directory as the starting point
                string root = Path.GetPathRoot(searchPattern);
                if (!String.IsNullOrEmpty(root))
                {
                    searchPattern = searchPattern.Substring(root.Length);
                }
                else
                {
                    root = baseDirectory;
                }

                // Push the root as the current level of the search
                currentLevel.Add(root);

                // Split the pattern into each directory level (RemoveEmptyEntries also cleans up
                // two or more contiguos directory separators, e.g. c:\foo\\bar, as well as any
                // starting or ending separator).
                string[] tokens = searchPattern.Split(new char[] { Path.DirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                // For each token we have, will we have base paths to search from
                for (int i = 0; i < tokens.Length && currentLevel.Count > 0; i++)
                {
                    string token = tokens[i];
                    bool isLastToken = (i == tokens.Length - 1);

                    // Search for directories with every token, except for the last. At that point
                    // we need to return the search type passed as an argument to the method
                    SearchType currentSearchType = (isLastToken) ? searchType : SearchType.Directories;
                    currentLevel = GetDescendants(currentLevel, token, currentSearchType);
                }

                // At the end of the loops, the current level will contain the result
                return currentLevel;
            }
        }

        /// <summary>
        /// Determines whether the specified value contains wildcards.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value contains wildcards; otherwise, <c>false</c>.
        /// </returns>
        private static bool ContainsWildcards(string value)
        {
            return value.Contains("?") || value.Contains("*") || value.Contains("...");
        }

        /// <summary>
        /// Gets the direct descendants of a given set of base paths that match a given search pattern.
        /// If the recursive wildcard (...) is used, then all descendants are searched for, not only the
        /// direct ones.
        /// </summary>
        /// <param name="basePaths">The base paths that will be searched.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchType">Defines the type of file system entry being searched for.</param>
        /// <returns>A collection of matching files/directories for all the input base paths.</returns>
        private static ICollection<string> GetDescendants(ICollection<string> basePaths, string searchPattern, SearchType searchType)
        {
            List<string> results = new List<string>();
            bool patternHasWildcards = ContainsWildcards(searchPattern);

            foreach (string basePath in basePaths)
            {
                if (!Directory.Exists(basePath))
                {
                    continue;
                }

                if (patternHasWildcards)
                {
                    results.AddRange(GetDescendants(basePath, searchPattern, searchType));
                }
                else
                {
                    string next = Path.Combine(basePath, searchPattern);
                    if (PathMatches(next, searchType))
                    {
                        results.Add(next);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Gets the direct descendants of a given set base path that match a given search pattern.
        /// If the recursive wildcard (...) is used, then all descendants are searched for, not only the
        /// direct ones.
        /// </summary>
        /// <param name="path">The starting base path.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <param name="searchType">Defines the type of file system entry being searched for.</param>
        /// <returns>A collection of matching files/directories for the input base path.</returns>
        private static string[] GetDescendants(string path, string searchPattern, SearchType searchType)
        {
            try
            {
                // Recursive wildcard
                if (searchPattern.Equals("..."))
                {
                    if (searchType == SearchType.Directories)
                    {
                        return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                    }
                    else if (searchType == SearchType.Files)
                    {
                        return Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    }

                    // KLUDGE: Directory doesn't provide a Directory.GetFileSystemEntries() that
                    // has a 3rd parameter. Emulate it on our own...
                    string[] dirs = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
                    string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
                    return LightMerge(dirs, files);
                }
                else
                {
                    if (searchType == SearchType.Directories)
                    {
                        return Directory.GetDirectories(path, searchPattern);
                    }
                    else if (searchType == SearchType.Files)
                    {
                        return Directory.GetFiles(path, searchPattern);
                    }

                    return Directory.GetFileSystemEntries(path, searchPattern);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }

            // If we got here, there was an IO error and an access control error,
            // return 0 results...
            return new string[0];
        }

        /// <summary>
        /// Merges two arrays in an optimized way when one of the arrays is of zero length.
        /// If arrays need to be merged, then the result will also be sorted.
        /// </summary>
        /// <param name="array1">One array.</param>
        /// <param name="array2">Another array.</param>
        /// <returns>The resulting array (will be one of the input arrays if the other
        /// was empty).</returns>
        private static string[] LightMerge(string[] array1, string[] array2)
        {
            if (array1.Length == 0)
            {
                return array2;
            }

            if (array2.Length == 0)
            {
                return array1;
            }

            string[] result = new string[array1.Length + array2.Length];
            array1.CopyTo(result, 0);
            array2.CopyTo(result, array1.Length);
            Array.Sort<string>(result);
            return result;
        }

        /// <summary>
        /// Determines if a given path matches a given search type (if it is an existing file 
        /// or directory).
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="searchType">Defines the type of file system entry being searched for.</param>
        /// <returns><c>true</c> if the path matches the search type; otherwise <c>false</c>.</returns>
        private static bool PathMatches(string path, SearchType searchType)
        {
            switch (searchType)
            {
                case SearchType.Directories:
                    return Directory.Exists(path);

                case SearchType.Files:
                    return File.Exists(path);

                case SearchType.FilesAndDirectories:
                    return Directory.Exists(path) || File.Exists(path);
            }

            throw new InvalidOperationException("Unrecognized search type: " + searchType);
        }

        /// <summary>
        /// Defines constants for scoping a search for files only, directories only,
        /// or both.
        /// </summary>
        private enum SearchType
        {
            Files, Directories, FilesAndDirectories
        }
    }

    /// <summary>
    /// Affects how file and directory deletions occur.
    /// </summary>
    [Flags]
    public enum DeleteMode
    {
        None,

        /// <summary>
        /// Force deletes read-only files by changing their bit before deleting.
        /// </summary>
        Force = 0x01
    }
}
