// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Sandbox.Caching
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
