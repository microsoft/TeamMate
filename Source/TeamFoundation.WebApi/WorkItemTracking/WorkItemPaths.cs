using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.TeamFoundation.WebApi.WorkItemTracking
{
    public static class WorkItemPaths
    {
        public const char PathSeparatorChar = '\\';
        public const string PathSeparator = "\\";
        private static readonly char[] PathSeparatorChars = new char[] { PathSeparatorChar };

        private static readonly StringComparison PathComparison = StringComparison.OrdinalIgnoreCase;

        public static bool IsPathUnder(string path, string under)
        {
            return AreEqual(path, under)
                || (path.Length > under.Length && path.StartsWith(under, PathComparison) && path[under.Length] == PathSeparatorChar);
        }

        public static bool AreEqual(string path, string under)
        {
            return String.Equals(path, under, PathComparison);
        }

        public static string FindCommonBasePath(IEnumerable<string> paths)
        {
            string commonBasePath = null;

            foreach (var path in paths)
            {
                commonBasePath = (commonBasePath == null) ? path : FindCommonBasePath(commonBasePath, path);

                // If we already found out there was no common path, exit early
                if (commonBasePath == null)
                {
                    break;
                }
            }

            return commonBasePath;
        }

        public static string FindCommonBasePath(string path1, string path2)
        {
            if (AreEqual(path1, path2))
            {
                return path1;
            }

            if (path1.Length < path2.Length && IsPathUnder(path2, path1))
            {
                return path1;
            }

            if (path2.Length < path1.Length && IsPathUnder(path1, path2))
            {
                return path2;
            }

            var path1Tokens = SplitPath(path1);
            var path2Tokens = SplitPath(path2);

            int maxTokens = Math.Min(path1Tokens.Length, path2Tokens.Length);

            int matchingTokenCount;
            for (matchingTokenCount = 0; matchingTokenCount < maxTokens; matchingTokenCount++)
            {
                if (!AreEqual(path1Tokens[matchingTokenCount], path2Tokens[matchingTokenCount]))
                {
                    break;
                }
            }

            if (matchingTokenCount > 0)
            {
                return String.Join(PathSeparator, path1Tokens.Take(matchingTokenCount));
            }

            return null;
        }

        public static string[] SplitPath(string path)
        {
            return path.Split(PathSeparatorChars, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
