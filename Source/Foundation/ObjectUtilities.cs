using System;

namespace Microsoft.Internal.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility mehtods for objects.
    /// </summary>
    public static class ObjectUtilities
    {
        // TODO: Do we really need this class?

        /// <summary>
        /// Determines whether an object is a null or empty string.
        /// </summary>
        /// <param name="value">The value.</param>
        public static bool IsNullOrEmptyString(object value)
        {
            return value == null || (value is string && String.IsNullOrEmpty((string)value));
        }

        /// <summary>
        /// Determines whether an object is a null or whitespace string.
        /// </summary>
        /// <param name="value">The value.</param>
        public static bool IsNullOrWhiteSpaceString(object value)
        {
            return value == null || (value is string && String.IsNullOrWhiteSpace((string)value));
        }
    }
}
