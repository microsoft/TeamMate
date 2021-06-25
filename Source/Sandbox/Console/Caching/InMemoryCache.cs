using System.Collections.Generic;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Caching
{
    public class InMemoryCache : ICache
    {
        private Dictionary<string, byte[]> entries = new Dictionary<string, byte[]>();

        public bool Contains(string key)
        {
            return entries.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return entries.Remove(key);
        }

        public void Add(string key, byte[] value)
        {
            entries[key] = value;
        }

        public void Clear()
        {
            entries.Clear();
        }

        public void Dispose()
        {
        }

        public byte[] Get(string key)
        {
            return entries[key];
        }
    }
}
