using System;
using System.Globalization;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Applies a percentage to an input value.
    /// </summary>
    public class PercentageConverter : OneWayConverterBase
    {
        /// <summary>
        /// An optional percentage value to use, if a parameter is not passed in.
        /// </summary>
        public double? Percentage { get; set; }

        /// <summary>
        /// Returns a percentage of the input value, using the parameter as a percentage factor (0-1),
        /// or the Percentage property.
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double? percentage = null;

            if (parameter is double)
            {
                percentage = (double)parameter;
            }
            else if (parameter != null)
            {
                percentage = Double.Parse(parameter.ToString(), CultureInfo.InvariantCulture);
            }
            else if (Percentage != null)
            {
                percentage = Percentage;
            }

            if (percentage != null)
            {
                double doubleValue = (double)value;
                return doubleValue * percentage;
            }
            else
            {
                return value;
            }
        }
    }
}
