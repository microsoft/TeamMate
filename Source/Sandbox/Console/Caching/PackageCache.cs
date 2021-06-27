using Microsoft.Tools.TeamMate.Foundation.IO.Packaging;
using Microsoft.Tools.TeamMate.Foundation.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;

namespace Microsoft.Tools.TeamMate.Sandbox.Caching
{
    public class PackageCache : ICache
    {
        private const string CacheFileName = "Cache.xml";

        private string packageFile;
        private IDictionary<string, string> keyToFileMap = new Dictionary<string, string>();
        private Package package;

        public PackageCache(string packageFile)
        {
            this.packageFile = packageFile;
            this.package = Package.Open(packageFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            var stream = GetPartStream(CacheFileName);
            if (stream != null)
            {
                using (stream)
                {
                    this.keyToFileMap = ReadCacheFile(stream);
                }
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
                var part = TryGetPart(filename);
                if (part != null)
                {
                    part.Delete();
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

            using (Stream stream = GetPartStream(filename, false))
            {
                MemoryStream source = new MemoryStream(value);
                source.CopyTo(stream);
            }

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
            foreach (var part in package.GetParts().ToArray())
            {
                part.Delete();
            }

            Flush();
        }

        private void Flush()
        {
            using (Stream stream = GetPartStream(CacheFileName, false))
            {
                WriteCacheFile(this.keyToFileMap, stream);
            }
        }

        private PackagePart TryGetPart(string partName)
        {
            var partUri = GetPartUri(partName);
            return this.package.TryGetPart(partUri);
        }

        private static Uri GetPartUri(string partName)
        {
            return PackUriHelper.CreatePartUri(new Uri("/" + partName, UriKind.Relative));
        }

        private Stream GetPartStream(string partName, bool read = true)
        {
            var partUri = GetPartUri(partName);
            var part = this.package.TryGetPart(partUri);
            if (read)
            {
                if (part != null)
                {
                    return part.OpenRead();
                }
            }
            else
            {
                if (part == null)
                {
                    // TODO: What is the correct content type here? Try to extract from extension?
                    part = this.package.CreatePart(partUri, "application/octet-stream");
                }

                return part.OpenWrite();
            }

            return null;
        }

        private static IDictionary<string, string> ReadCacheFile(Stream stream)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            var doc = XDocument.Load(stream);
            var entries = doc.Elements("Cache", "Entry");
            foreach (var entry in entries)
            {
                string key = (string)entry.Attribute("Key");
                string filename = (string)entry.Attribute("Filename");
                result[key] = filename;
            }

            return result;
        }

        private static void WriteCacheFile(IDictionary<string, string> keyToFileMap, Stream stream)
        {
            XDocument doc = new XDocument(
                new XElement("Cache",
                    keyToFileMap.OrderBy(e => e.Key).Select(e => new XElement("Entry",
                        new XAttribute("Key", e.Key),
                        new XAttribute("Filename", e.Value)
                    ))
                )
            );

            doc.Save(stream);
        }

        public void Dispose()
        {
            this.package.Close();
        }

        public byte[] Get(string key)
        {
            string filename;
            if (keyToFileMap.TryGetValue(key, out filename))
            {
                Stream stream = GetPartStream(filename);
                if (stream != null)
                {
                    MemoryStream target = new MemoryStream();
                    using (stream)
                    {
                        stream.CopyTo(target);
                    }

                    return target.GetBuffer();
                }
            }

            throw new KeyNotFoundException();
        }
    }
}
