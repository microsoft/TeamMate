// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// A converter that converts objects to boolean values.
    /// </summary>
    public class BooleanConverter : OneWayConverterBase
    {
        /// <summary>
        /// <c>true</c> to inverse (negate) the result of the conversion.
        /// </summary>
        public bool Inverse { get; set; }

        /// <summary>
        /// Converts the specified value to a boolean value.
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = ToBoolean(value);
            return (Inverse) ? !result : result;
        }

        /// <summary>
        /// Converts an object to a boolean value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The corresponding boolean value for the input object.</returns>
        /// <remarks>
        /// The conversion rules are as follows:
        /// 1) If the input is a boolean value, return it.
        /// 2) If the input is a string, return true if not null or empty.
        /// 3) If the input is a collection, return true if not empty.
        /// 4) If the input is an enumerable, return true if not empty;
        /// 5) If the input is an integer or double, return true if != 0.
        /// 6) If the input is not null, return true.
        /// </remarks>
        public static bool ToBoolean(object value)
        {
            if (value is bool)
            {
                return (bool)value;
            }

            if (value is string)
            {
                return !String.IsNullOrEmpty((string)value);
            }

            if (value is System.Collections.ICollection)
            {
                System.Collections.ICollection collection = (System.Collections.ICollection)value;
                return (collection.Count > 0);
            }

            if (value is System.Collections.IEnumerable)
            {
                System.Collections.IEnumerable enumerable = (System.Collections.IEnumerable)value;
                return (enumerable.OfType<object>().Any());
            }

            if (value is int)
            {
                return (int)value != 0;
            }

            if (value is double)
            {
                return (double)value != 0;
            }

            return (value != null);
        }
    }
}
