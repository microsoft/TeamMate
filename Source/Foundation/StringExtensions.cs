using Microsoft.Tools.TeamMate.Foundation.Diagnostics;
using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides extension methods for strings.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Determines whether a string contains the specificied value, applying a comparison type.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="value">The value.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <returns><c>true</c> if the string contained the specified value.</returns>
        public static bool Contains(this string text, string value, StringComparison comparisonType)
        {
            Assert.ParamIsNotNull(text, "text");

            return text.IndexOf(value, comparisonType) >= 0;
        }
    }
}
