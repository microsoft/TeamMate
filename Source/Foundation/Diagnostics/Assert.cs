using System;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Diagnostics
{
    /// <summary>
    /// A helper static class used to assert common conditions.
    /// </summary>
    public static class Assert
    {
        /// <summary>
        /// Asserts that a given parameter value satisfies an arbitrary assertion.
        /// </summary>
        /// <param name="assertion">If false, then an argument exception will be thrown.</param>
        /// <param name="paramName">The parameter name.</param>
        /// <param name="message">The message to throw as part of the assertion.</param>
        public static void ParamIs(bool assertion, string paramName, string message)
        {
            if (!assertion)
            {
                throw new ArgumentException(message, paramName);
            }
        }

        /// <summary>
        /// Asserts that a given parameter value is not <c>null</c>.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsNotNull(object value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Asserts that a given string parameter value is not <c>null</c> or empty.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsNotNullOrEmpty(string value, string paramName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(paramName);
            }

            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException("String parameter cannot be empty", paramName);
            }
        }

        /// <summary>
        /// Asserts that a given parameter is equal to or greater than 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsNotNegative(int value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Parameter must be equal to or greater than 0");
            }
        }

        /// <summary>
        /// Asserts that a given parameter is equal to or greater than 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsNotNegative(long value, string paramName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Parameter must be equal to or greater than 0");
            }
        }

        /// <summary>
        /// Asserts that a given parameter is equal to or greater than 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsNotNegative(TimeSpan value, string paramName)
        {
            if (value < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "TimeSpan must be equal to or greater than 0");
            }
        }

        /// <summary>
        /// Asserts that a given parameter is greater than 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsGreaterThanZero(int value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Parameter must greater than 0");
            }
        }

        /// <summary>
        /// Asserts that a given parameter is greater than 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsGreaterThanZero(TimeSpan value, string paramName)
        {
            if (value <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "TimeSpan must greater than 0");
            }
        }

        /// <summary>
        /// Asserts that a given parameter is greater than 0.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsGreaterThanZero(double value, string paramName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Value must greater than 0");
            }
        }

        /// <summary>
        /// Asserts that a given parameter is within a range (inclusive of the low and high bounds).
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="lowBound">The low bound.</param>
        /// <param name="highBound">The high bound.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsWithinRange(double value, double lowBound, double highBound, string paramName)
        {
            if (value < lowBound || value > highBound)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Parameter must be a value between " + lowBound + " and " + highBound);
            }
        }

        /// <summary>
        /// Asserts that a given parameter is a valid index for a given count or length of collection.
        /// </summary>
        /// <param name="value">The parameter value.</param>
        /// <param name="count">The maximum count.</param>
        /// <param name="paramName">The parameter name.</param>
        public static void ParamIsValidIndex(int value, int count, string paramName)
        {
            if (value < 0 || value >= count)
            {
                throw new ArgumentOutOfRangeException(paramName, value, "Index was out of range. Must be non-negative and less than the size of the collection.");
            }
        }
    }
}
