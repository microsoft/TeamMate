// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows.Controls.Data
{
    /// <summary>
    /// A converter used to render the group names of a list group with an item count.
    /// </summary>
    public class GroupNameConverter : IMultiValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GroupNameConverter"/> class.
        /// </summary>
        public GroupNameConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupNameConverter"/> class.
        /// </summary>
        /// <param name="mode">The string conversion mode.</param>
        public GroupNameConverter(StringConverterMode mode)
        {
            this.Mode = mode;
        }

        public StringConverterMode Mode { get; set; }


        /// <summary>
        /// Expects two input values (a group name and an item count), and formats those as an output string of the form
        /// "My Group (12)".
        /// </returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string groupName = (values.Length > 0 && values[0] != null && values[0] != DependencyProperty.UnsetValue) ? values[0].ToString() : null;

            if (!String.IsNullOrEmpty(groupName))
            {
                int itemCount = (values.Length > 1 && values[1] is int) ? (int)values[1] : 0;

                string result = (itemCount > 0) ? String.Format("{0} ({1})", groupName, itemCount) : String.Format("{0}", groupName);
                return StringConverter.Convert(result, Mode);
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
