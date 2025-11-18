using System;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class StatusService
    {
        public IDisposable BusyIndicator()
        {
            return new Microsoft.Tools.TeamMate.Foundation.Windows.Forms.TemporaryGlobalCursor();
        }
    }
}
