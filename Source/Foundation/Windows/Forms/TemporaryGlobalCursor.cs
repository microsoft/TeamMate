// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Windows.Forms;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Forms
{
    public class TemporaryGlobalCursor : IDisposable
    {
        private bool disposed;
        private Cursor originalCursor;

        public TemporaryGlobalCursor()
        {
            this.originalCursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!disposed)
            {
                Cursor.Current = this.originalCursor;
                this.originalCursor = null;
                disposed = true;
            }
        }

        #endregion
    }
}
