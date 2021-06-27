using System;

namespace Microsoft.Tools.TeamMate.Services
{
    public class StatusService
    {
        public IDisposable BusyIndicator()
        {
            return new Microsoft.Tools.TeamMate.Foundation.Windows.Forms.TemporaryGlobalCursor();
        }
    }
}
