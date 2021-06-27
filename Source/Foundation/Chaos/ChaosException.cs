// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Foundation.Chaos
{
    /// <summary>
    /// An exception thrown by the chaos monkey whenever it introduces chaos into our product.
    /// </summary>
    public class ChaosException : Exception
    {
        public ChaosException()
            : base("The chaos monkey was naughty and injected this exception!")
        {
        }

        public ChaosException(string message)
            : base(message)
        {
        }

        public ChaosException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
