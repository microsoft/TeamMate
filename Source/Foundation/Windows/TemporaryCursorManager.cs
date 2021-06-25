using System;
using System.Windows;
using System.Windows.Input;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    internal class TemporaryCursorManager : IDisposable
    {
        private FrameworkElement e;
        private Cursor originalCursor;
        private bool originalForceCursor;
        private bool disposed;

        public TemporaryCursorManager(FrameworkElement e, Cursor newCursor)
        {
            this.e = e;
            this.originalCursor = e.Cursor;
            this.originalForceCursor = e.ForceCursor;
            this.e.Cursor = newCursor;
            this.e.ForceCursor = true;
        }
        #region IDisposable Members

        public void Dispose()
        {
            if (!disposed)
            {
                this.e.ForceCursor = originalForceCursor;
                this.e.Cursor = originalCursor;
                disposed = true;
            }
        }

        #endregion
    }
}
