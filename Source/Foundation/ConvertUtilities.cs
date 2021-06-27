// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace Microsoft.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods for converting types.
    /// </summary>
    public static class ConvertUtilities
    {
        /// <summary>
        /// Changes an input value to the target type.
        /// </summary>
        /// <typeparam name="T">The target type.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The converted type.</returns>
        public static T ChangeType<T>(object value)
        {
            Type targetType = typeof(T);
            object result = null;

            // In the future, add more types here (e.g. Version, TimeSpan, DateTime, etc...)
            if (targetType == typeof(Uri) && value is string)
            {
                result = new Uri((string)value);
            }

            if (result == null)
            {
                result = Convert.ChangeType(value, targetType);
            }

            return (T)result;
        }
    }
}
