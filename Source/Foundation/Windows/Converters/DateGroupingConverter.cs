// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// A converter that converts date objects to friendly date periods.
    /// </summary>
    public class DateGroupingConverter : OneWayConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                return "Undefined";
            }

            DateTime date = ((DateTime)value);
            return date.ToFriendlyDatePeriod();
        }
    }
}
