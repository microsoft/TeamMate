// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Shell
{
    public class ShellFileInfoCache
    {
        private IDictionary<string, ShellFileInfo> extensionCache =
            new Dictionary<string, ShellFileInfo>(StringComparer.OrdinalIgnoreCase);

        private IDictionary<string, ShellFileInfo> fileCache =
            new Dictionary<string, ShellFileInfo>(StringComparer.OrdinalIgnoreCase);

        private IDictionary<string, ShellFolderInfo> folderCache =
            new Dictionary<string, ShellFolderInfo>(StringComparer.OrdinalIgnoreCase);

        private ShellFolderInfo defaultFolderInfo;
        private ShellFileInfo defaultFileInfo;
        private object cacheLock = new object();

        public ShellFileInfoCache()
        {
            LoadLargeIcons = true;
        }

        public bool LoadLargeIcons { get; set; }

        public ShellFileInfo DefaultFileInfo
        {
            get
            {
                if (defaultFileInfo == null)
                    defaultFileInfo = ShellFileInfo.GetDefaultFileInfo();

                return defaultFileInfo;
            }
        }

        public ShellFolderInfo DefaultFolderInfo
        {
            get
            {
                if (defaultFolderInfo == null)
                    defaultFolderInfo = ShellFileInfo.GetDefaultFolderInfo();

                return defaultFolderInfo;
            }
        }

        public ShellFileInfo FromExtension(string pathOrExtension)
        {
            return GetOrCreate<ShellFileInfo>(pathOrExtension, extensionCache, (p) => ShellFileInfo.FromExtension(p, LoadLargeIcons));
        }

        public ShellFileInfo FromFile(string path)
        {
            return GetOrCreate<ShellFileInfo>(path, fileCache, (p) => ShellFileInfo.FromFile(p, LoadLargeIcons));
        }

        public ShellFolderInfo FromDirectory(string path)
        {
            return GetOrCreate<ShellFolderInfo>(path, folderCache, (p) => ShellFileInfo.FromDirectory(p, LoadLargeIcons));
        }

        private T GetOrCreate<T>(string path, IDictionary<string,T> cache, Func<string, T> constructor)
        {
            T output;
            bool exists;

            lock (cacheLock)
            {
                exists = cache.TryGetValue(path, out output);
            }


            if (!exists)
            {
                output = constructor(path);
                if (output != null)
                {
                    lock (cacheLock)
                    {
                        cache[path] = output;
                    }
                }
            }

            return output;
        }

        public void Clear()
        {
            lock (cacheLock)
            {
                extensionCache.Clear();
                fileCache.Clear();
                folderCache.Clear();
                defaultFileInfo = null;
                defaultFolderInfo = null;
            }
        }

        public ICollection<string> CachedExtensions
        {
            get { return extensionCache.Keys; }
        }

        public ICollection<string> CachedFiles
        {
            get { return fileCache.Keys; }
        }

        public ICollection<string> CachedFolders
        {
            get { return folderCache.Keys; }
        }
    }
}
