using System;
using System.Windows;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Extracts the left component of a thickness value.
    /// </summary>
    public class ThicknessValueConverter : OneWayConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double result = 0;

            if (value != null)
            {
                Thickness thickness = (Thickness) value;
                result = thickness.Left;
            }

            return result;
        }
    }
}
