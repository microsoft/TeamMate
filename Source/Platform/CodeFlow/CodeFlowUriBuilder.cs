using Microsoft.Tools.TeamMate.Foundation.Win32;
using System;

namespace Microsoft.Tools.TeamMate.Platform.CodeFlow
{
    public static class CodeFlowUriBuilder
    {
        private const string CodeFlowProtocolName = "codeflow";
        private const string CodeFlowSiteUrl = "http://codeflow";

        // TODO: Some overlap between these and the CodeFlowService. Some of these are "defaults"? Some of them are discovered.
        private const string ClientUrl           = CodeFlowSiteUrl      + "/Client/CodeFlow2010.application";
        private const string CodeFlowLauncherUrl = CodeFlowProtocolName + ":open";
        private const string VsClientUrl         = CodeFlowSiteUrl      + "/VSClient/VsCodeFlowLauncher.application";
        private const string DiscoveryServiceUrl = CodeFlowSiteUrl      + "/Services/DiscoveryService.svc";
        private const string DashboardUrl        = CodeFlowSiteUrl      + "/dashboard";
        private const string WebDisplayUrlFormat = DashboardUrl         + "/review/display/{0}";

        private const string ReviewUrlFormat = "{0}?server={1}&review={2}";

        public static Uri Dashboard()
        {
            return CreateUri(DashboardUrl);
        }

        public static Uri LaunchClient()
        {
            return CreateUri(ClientUrl);
        }

        /// <summary>
        /// Constructs a Uri that will be used to launch CodeFlow application
        /// If the CodeFlow protocol handler is installed, then this method
        /// creates and returns a codeflow:// URI, otherwise it will 
        /// return a http:// URI. 
        /// </summary>
        /// <param name="reviewKey"></param>
        /// <returns>
        /// If a protocol handler for codeflow:// is found, then the returned Uri
        /// will be of the form:
        ///     codeflow:open?server=http://codeflow/Services/DiscoveryService.svc&review=<review-key>
        /// Otherwise, the return value will be of the form:
        ///     http://codeflow/Client/CodeFlow2010.application?server=http://codeflow/Services/DiscoveryService.svc&review=<review-key>
        /// </returns>
        public static Uri LaunchClient(string reviewKey)
        {
            string launcherUrl = ProtocolUtilities.HandlerExists(CodeFlowProtocolName) ? CodeFlowLauncherUrl : ClientUrl;

            // KLUDGE: BY DESIGN TO NOT ESCAPE THE FIRST PARAMETERS (unfortunately). Otherwise, CodeFlow throws:
            // System.UriFormatException: Invalid URI: The format of the URI could not be determined.
            return CreateUri(ReviewUrlFormat, launcherUrl, DiscoveryServiceUrl, Uri.EscapeDataString(reviewKey));
        }

        public static Uri LaunchVisualStudio()
        {
            return CreateUri(VsClientUrl);
        }

        public static Uri LaunchVisualStudio(string reviewKey)
        {
            // KLUDGE: BY DESIGN TO NOT ESCAPE THE FIRST PARAMETERS (unfortunately). Otherwise, CodeFlow throws:
            // System.UriFormatException: Invalid URI: The format of the URI could not be determined.
            return CreateUri(ReviewUrlFormat, VsClientUrl, DiscoveryServiceUrl, Uri.EscapeDataString(reviewKey));
        }

        public static Uri WebView(string reviewKey)
        {
            return CreateUri(WebDisplayUrlFormat, Uri.EscapeDataString(reviewKey));
        }

        private static Uri CreateUri(string format, params object[] args)
        {
            return new Uri(String.Format(format, args), UriKind.Absolute);
        }
    }
}
