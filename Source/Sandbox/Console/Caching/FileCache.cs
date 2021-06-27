// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.IO;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Sandbox.Caching
{
    public class FileCache : ICache
    {
        private const string CacheFileName = "Cache.xml";

        private string cacheFolder;
        private IDictionary<string, string> keyToFileMap = new Dictionary<string, string>();

        public FileCache(string cacheFolder)
        {
            this.cacheFolder = cacheFolder;
            PathUtilities.EnsureDirectoryExists(this.cacheFolder);

            // Load cache file
            string cacheFile = GetFullPath(CacheFileName);
            if (File.Exists(cacheFile))
            {
                this.keyToFileMap = ReadCacheFile(cacheFile);
            }
        }

        public bool Contains(string key)
        {
            return keyToFileMap.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            string filename;
            if (keyToFileMap.TryGetValue(key, out filename))
            {
                string fullPath = GetFullPath(filename);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }

                Flush();
                return true;
            }

            return false;
        }

        public void Add(string key, byte[] value)
        {
            bool isNewEntry = false;

            string filename;
            if (!keyToFileMap.TryGetValue(key, out filename))
            {
                filename = Guid.NewGuid().ToString() + ".cache";
                isNewEntry = true;
            }

            string fullPath = GetFullPath(filename);
            File.WriteAllBytes(fullPath, value);

            // Only write new entry if we succeeded to save to disk
            if (isNewEntry)
            {
                keyToFileMap[key] = filename;
                Flush();
            }
        }

        public void Clear()
        {
            keyToFileMap.Clear();
            PathUtilities.DeleteContents(this.cacheFolder);
            Flush();
        }

        private void Flush()
        {
            string cacheFile = GetFullPath(CacheFileName);
            WriteCacheFile(this.keyToFileMap, cacheFile);
        }

        private string GetFullPath(string filename)
        {
            return Path.Combine(this.cacheFolder, filename);
        }

        private static IDictionary<string, string> ReadCacheFile(string path)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var doc = XDocument.Load(path);
            var entries = doc.Elements("Cache", "Entry");
            foreach (var entry in entries)
            {
                string key = (string)entry.Attribute("Key");
                string filename = (string)entry.Attribute("Filename");
                result[key] = filename;
            }

            return result;
        }

        private static void WriteCacheFile(IDictionary<string, string> keyToFileMap, string path)
        {
            XDocument doc = new XDocument(
                new XElement("Cache",
                    keyToFileMap.OrderBy(e => e.Key).Select(e => new XElement("Entry",
                        new XAttribute("Key", e.Key),
                        new XAttribute("Filename", e.Value)
                    ))
                )
            );

            doc.Save(path);
        }

        public void Dispose()
        {
        }

        public byte[] Get(string key)
        {
            string filename;
            if (keyToFileMap.TryGetValue(key, out filename))
            {
                return File.ReadAllBytes(GetFullPath(filename));
            }

            throw new KeyNotFoundException();
        }
    }
}
