using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;
using System.Collections.Generic;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods for working with URIs.
    /// </summary>
    public static class UriUtilities
    {
        private const string PathSeparator = "/";

        public static readonly IEqualityComparer<Uri> AbsoluteUriComparer = new AbsoluteUriComparer();

        /// <summary>
        /// Combines two URI paths.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="path2">Another path.</param>
        /// <returns>The combined URI paths.</returns>
        public static string CombinePaths(string path, string path2)
        {
            Assert.ParamIsNotNull(path, "path");
            Assert.ParamIsNotNull(path2, "path2");

            if (path2.StartsWith(PathSeparator))
            {
                return path2;
            }

            string result = path;
            if (!result.EndsWith(PathSeparator))
            {
                result += PathSeparator;
            }

            result += path2;
            return result;
        }

        /// <summary>
        /// Checks if to absolute URIs are equal.
        /// </summary>
        /// <param name="uri1">A uri.</param>
        /// <param name="uri2">Another uri.</param>
        /// <returns><c>true</c> if the objects where equal, otherwise <c>false</c>.</returns>
        public static bool AreEqual(Uri uri1, Uri uri2)
        {
            if (uri1 == null || uri2 == null)
            {
                return Object.Equals(uri1, uri2);
            }

            return (uri1 == uri2) || AbsoluteUriComparer.Equals(uri1, uri2);
        }

        /// <summary>
        /// Determines whether the given URI is an HTTP or HTTPS URI.
        /// </summary>
        /// <param name="uri">The URI.</param>
        /// <returns><c>true</c> if the URI corresponds to the HTTP(S) protocol.</returns>
        public static bool IsHttpUri(Uri uri)
        {
            Assert.ParamIsNotNull(uri, "uri");

            return uri.IsAbsoluteUri && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }
    }
}
