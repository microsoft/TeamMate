// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    public class LazyWeakReference<T> where T : class
    {
        private Func<T> valueFactory;
        private WeakReference weakRefence = new WeakReference(null);

        public LazyWeakReference(Func<T> valueFactory)
        {
            this.valueFactory = valueFactory;
        }

        public T Value
        {
            get
            {
                T result = (T)weakRefence.Target;
                if (result == null)
                {
                    result = valueFactory();
                    weakRefence.Target = result;
                }

                return result;
            }
        }

        public bool IsValueCreated
        {
            get
            {
                return weakRefence != null && weakRefence.IsAlive;
            }
        }
    }
}
