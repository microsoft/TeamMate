using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Model;
using System.Runtime.Versioning;

namespace Microsoft.Tools.TeamMate.Services
{
    [SupportedOSPlatform("windows10.0.19041.0")]
    public class ExternalWebBrowserService
    {
        public void OpenGitHubProjectUrl()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.GitHubProjectUri);
        }

        public void OpenGitHubMITLicenseUrl()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.GitHubMITLicenseUri);
        }
    }
}
