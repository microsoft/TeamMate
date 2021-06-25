using System;
using System.Globalization;
using System.Windows;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Converts an input value to a visibility value.
    /// </summary>
    public class VisibilityConverter : OneWayConverterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityConverter"/> class.
        /// </summary>
        public VisibilityConverter()
        {
            WhenInvisible = Visibility.Collapsed;
        }

        /// <summary>
        /// <c>true</c> to inverse the result of the conversion.
        /// </summary>
        public bool Inverse { get; set; }

        /// <summary>
        /// Gets or sets the target result to convert to when invisible.
        /// </summary>
        public Visibility WhenInvisible { get; set; }

        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible;

            if (parameter == null)
            {
                visible = BooleanConverter.ToBoolean(value);
            }
            else
            {
                visible = Object.Equals(value, parameter);
            }

            if (visible)
            {
                return (!Inverse) ? Visibility.Visible : WhenInvisible;
            }
            else
            {
                return (!Inverse) ? WhenInvisible : Visibility.Visible;
            }
        }
    }
}
