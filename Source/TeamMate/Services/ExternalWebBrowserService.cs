using Microsoft.Tools.TeamMate.Foundation.Shell;
using Microsoft.Tools.TeamMate.Model;

namespace Microsoft.Tools.TeamMate.Services
{
    public class ExternalWebBrowserService
    {
        public void OpenGitHubProjectUrl()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.GitHubProjectUri);
        }
    }
}
