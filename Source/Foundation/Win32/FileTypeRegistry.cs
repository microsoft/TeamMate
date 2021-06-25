using Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Win32
{
    /// <summary>
    /// A registry that provides and caches information about file types.
    /// </summary>
    public class FileTypeRegistry
    {
        private Dictionary<string, FileTypeInfo> cache = new Dictionary<string, FileTypeInfo>(StringComparer.OrdinalIgnoreCase);

        private static Lazy<FileTypeRegistry> lazySingleton = new Lazy<FileTypeRegistry>(() => new FileTypeRegistry());

        /// <summary>
        /// Gets a global shared file type registry.
        /// </summary>
        public static FileTypeRegistry Instance
        {
            get { return lazySingleton.Value; }
        }

        /// <summary>
        /// Gets the file type information for a given extension.
        /// </summary>
        /// <param name="extension">The extension.</param>
        /// <returns>The file type information.</returns>
        public FileTypeInfo GetInfo(string extension)
        {
            Assert.ParamIsNotNull(extension, "extension");

            FileTypeInfo info;
            if (!cache.TryGetValue(extension, out info))
            {
                info = FileTypeInfo.FromExtension(extension);
                cache[extension] = info;
            }

            return info;
        }

        /// <summary>
        /// Gets the file type information for a given file path, by extracting the extension
        /// for the file path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns>The file type information.</returns>
        public FileTypeInfo GetInfoFromPath(string path)
        {
            Assert.ParamIsNotNull(path, "path");

            return GetInfo(Path.GetExtension(path));
        }

        /// <summary>
        /// Clears all of the cached file type information.
        /// </summary>
        public void ClearCache()
        {
            cache.Clear();
        }

        /// <summary>
        /// Removes an file type entry from the cache if it exists.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public void RemoveFromCache(string extension)
        {
            cache.Remove(extension);
        }

        /// <summary>
        /// Gets all registered extensions in the system (from the registry).
        /// </summary>
        public static ICollection<string> AllRegisteredExtensions
        {
            get
            {
                return Registry.ClassesRoot.GetSubKeyNames().Where(sk => sk.StartsWith(".")).ToArray();
            }
        }
    }
}
