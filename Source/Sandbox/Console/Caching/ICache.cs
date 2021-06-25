using System;

namespace Microsoft.Internal.Tools.TeamMate.Sandbox.Caching
{
    public interface ICache : IDisposable
    {
        bool Contains(string key);
        byte[] Get(string key);
        bool Remove(string key);
        void Add(string key, byte[] value);
        void Clear();
    }
}
