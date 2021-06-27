// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using Microsoft.Tools.TeamMate.Foundation.Native;
using System;

namespace Microsoft.Tools.TeamMate.Foundation.Runtime.InteropServices
{
    /// <summary>
    /// A helper class to use to define the scope of COM interop, setting a value to rety calls
    /// that are rejected due to busy COM servers while within the scope.
    /// </summary>
    public class RetryMessageFilterScope : IDisposable
    {
        private TimeSpan timeout;
        private RetryMessageFilter messageFilter;
        private IOleMessageFilter oldFilter;

        /// <summary>
        /// Creates a new instance with a default timeout.
        /// </summary>
        public RetryMessageFilterScope() : this(TimeSpan.FromSeconds(30))
        {
        }

        /// <summary>
        /// Creates a new instance with the given timeout.
        /// </summary>
        /// <param name="timeout">The timeout that will be used to retry failed calls.</param>
        public RetryMessageFilterScope(TimeSpan timeout)
        {
            Assert.ParamIsGreaterThanZero(timeout, "timeout");

            this.timeout = timeout;
            this.messageFilter = new RetryMessageFilter();
            this.messageFilter.RetryTimeout = timeout;

            // On construction, register a message filter that will retry busy calls
            // for the given timeout...
            this.oldFilter = this.messageFilter.Register();
        }

        /// <summary>
        /// Gets the timeout set on this scope.
        /// </summary>
        /// <value>The timeout.</value>
        public TimeSpan Timeout 
        {
            get { return this.timeout; }
        }

        #region IDisposable Members

        /// <summary>
        /// Terminates the scope and invalidates message filters and retry timeouts.
        /// </summary>
        public void Dispose()
        {
            if (this.messageFilter != null)
            {
                // Unregister the message filter (restoring any old filter that might have been registered)
                this.messageFilter.Unregister(this.oldFilter);
                this.messageFilter = null;
                this.oldFilter = null;
            }
        }

        #endregion
    }
}
