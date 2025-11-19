using Microsoft.Tools.TeamMate.Model;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class SessionService
    {
        public Session Session { get; set; } = new Session();
    }
}
