// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// A helper class to implement disposable patterns for API usability to execute
    /// an action when Dispose() is called.
    /// </summary>
    public class DelegateDisposable : IDisposable
    {
        private Action disposeAction;
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateDisposable"/> class.
        /// </summary>
        /// <param name="action">The action to be invoked on dispose.</param>
        public DelegateDisposable(Action action)
        {
            Assert.ParamIsNotNull(action, "disposeAction");

            this.disposeAction = action;
        }

        public void Dispose()
        {
            if(!isDisposed)
            {
                isDisposed = true;
                disposeAction();
            }
        }
    }
}
