using System;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class StatusService
    {
        public IDisposable BusyIndicator()
        {
            return new Microsoft.Internal.Tools.TeamMate.Foundation.Windows.Forms.TemporaryGlobalCursor();
        }
    }
}
