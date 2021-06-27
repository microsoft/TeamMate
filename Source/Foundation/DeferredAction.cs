// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    public class DeferredAction : IDisposable
    {
        private object sempahoreCountLock = new object();
        private int semaphoreCount;

        public DeferredAction(Action action)
        {
            Assert.ParamIsNotNull(action, "action");

            this.Action = action;
        }

        public bool IsDeferring
        {
            get
            {
                return this.semaphoreCount > 0;
            }
        }

        public Action Action { get; private set; }

        public void InvokeIfNotDeferred()
        {
            if (this.ShouldInvoke())
            {
                this.Invoke();
            }
        }

        public IDisposable Acquire()
        {
            lock (this.sempahoreCountLock)
            {
                this.semaphoreCount++;
            }

            return this;
        }

        private void Release()
        {
            bool shouldInvoke = false;

            lock (this.sempahoreCountLock)
            {
                if (this.semaphoreCount > 0)
                {
                    this.semaphoreCount--;
                    shouldInvoke = this.ShouldInvoke();
                }
            }

            if (shouldInvoke)
            {
                Invoke();
            }
        }

        private void Invoke()
        {
            this.Action.Invoke();
        }

        private bool ShouldInvoke()
        {
            lock (sempahoreCountLock)
            {
                return (this.semaphoreCount == 0);
            }
        }

        void IDisposable.Dispose()
        {
            this.Release();
        }
    }
}
