using Microsoft.Internal.Tools.TeamMate.Foundation.Reflection;
using System;
using System.Globalization;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// A converter that converts enum values to enum descriptions.
    /// </summary>
    public class EnumDisplayStringConverter : OneWayConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return ReflectionUtilities.GetEnumDescription(value);
            }

            return null;
        }
    }
}
