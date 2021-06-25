﻿using Microsoft.Internal.Tools.TeamMate.Foundation.Shell;
using Microsoft.Internal.Tools.TeamMate.Model;

namespace Microsoft.Internal.Tools.TeamMate.Services
{
    public class ExternalWebBrowserService
    {
        public void FileFeatureRequestOrBug()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.IssueListUrl);
        }

        public void VoteOnFeatures()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.IssueListUrl);
        }

        public void SuggestFeature()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.SuggestFeatureUrl);
        }

        public void ReportBug()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.ReportBugUrl);
        }

        public void ShowHelp()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.HelpUrl);
        }

        public void GoToToolbox()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.ToolboxHomeUrl);
        }

        public void GoToYammer()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.YammerUrl);
        }

        public void RateApplication()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.RatingUrl);
        }

        public void LaunchNewsPage()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.BlogUrl);
        }

        public void LaunchLegacyTfsSupportDropped()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.LegacyTfsSupportDroppedUrl);
        }

        public void ViewPrivacyStatemenet()
        {
            ExternalWebBrowser.Launch(TeamMateApplicationInfo.PrivacyStatementUrl);
        }
    }
}
