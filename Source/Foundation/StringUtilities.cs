using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Internal.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods for manipualting strings.
    /// </summary>
    public static class StringUtilities
    {
        private const char Comma = ',';
        private const string CommaSeparator = ", ";

        /// <summary>
        /// Converst an enumeration of items to a comma separated list.
        /// </summary>
        /// <param name="items">The items (can be null).</param>
        /// <returns>A joined string, or <c>null</c> if the input was <c>null</c>.</returns>
        public static string ToCommaSeparatedList(IEnumerable<string> items)
        {
            return (items != null) ? String.Join(CommaSeparator, items) : null;
        }

        /// <summary>
        /// Parses a comma-separated string and splits the values.
        /// </summary>
        /// <param name="text">The input text (can be null).</param>
        /// <returns>The string value, or <c>null</c> if the input was <c>null</c>.</returns>
        public static string[] FromCommaSeparatedList(string text)
        {
            string[] values = null;
            if (text != null)
            {
                values = text.Split(Comma).Select(i => i.Trim()).Where(i => !String.IsNullOrWhiteSpace(i)).ToArray();
            }
            return values;
        }
    }
}
