using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Diagnostics;

namespace Microsoft.Tools.TeamMate.Foundation.Shell
{
    /// <summary>
    /// Helper class for manipulating the system's web browser.
    /// </summary>
    public static class ExternalWebBrowser
    {
        // TODO: Rename to SystemWebBrowser?

        /// <summary>
        /// Launches the specified URI in the system's web browser.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public static void Launch(string uri)
        {
            Assert.ParamIsNotNullOrEmpty(uri, "uri");

            Launch(new Uri(uri));
        }

        /// <summary>
        /// Launches the specified URI in the system's web browser.
        /// </summary>
        /// <param name="uri">The URI.</param>
        public static void Launch(Uri uri)
        {
            Assert.ParamIsNotNull(uri, "uri");

            if (!uri.IsAbsoluteUri)
            {
                throw new ArgumentException("Can only launch absolute URIs");
            }

            // TODO: When run in a loop, not all URLs might be launched. See
            // http://stackoverflow.com/questions/6208307/process-starturl-in-a-loop-not-launching-every-instance
            string browserUri = uri.AbsoluteUri;
            var startInfo = new ProcessStartInfo(browserUri)
            {
                UseShellExecute = true
            };
            Process.Start(startInfo);
        }
    }
}
