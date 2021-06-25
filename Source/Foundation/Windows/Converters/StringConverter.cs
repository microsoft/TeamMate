using System;

namespace Microsoft.Internal.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Converts strings to lower and upper case variants.
    /// </summary>
    public class StringConverter : OneWayConverterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringConverter"/> class.
        /// </summary>
        public StringConverter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringConverter"/> class.
        /// </summary>
        /// <param name="mode">The mode.</param>
        public StringConverter(StringConverterMode mode)
        {
            this.Mode = mode;
        }

        /// <summary>
        /// Gets or sets the mode.
        /// </summary>
        public StringConverterMode Mode { get; set; }

        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string stringValue = (value != null) ? value.ToString() : String.Empty;
            return Convert(stringValue, Mode);
        }

        /// <summary>
        /// Converts the specified string value based on an input mode.
        /// </summary>
        /// <param name="stringValue">The string value.</param>
        /// <param name="mode">The conversion mode.</param>
        /// <returns>The converted string</returns>
        public static string Convert(string stringValue, StringConverterMode mode)
        {
            if(stringValue != null)
            {
                switch (mode)
                {
                    case StringConverterMode.ToLower:
                        stringValue = stringValue.ToLower();
                        break;

                    case StringConverterMode.ToLowerInvariant:
                        stringValue = stringValue.ToLowerInvariant();
                        break;

                    case StringConverterMode.ToUpper:
                        stringValue = stringValue.ToUpper();
                        break;

                    case StringConverterMode.ToUpperInvariant:
                        stringValue = stringValue.ToUpperInvariant();
                        break;
                }
            }

            return stringValue;
        }
    }

    /// <summary>
    /// Specifies the conversion mode for strings.
    /// </summary>
    public enum StringConverterMode
    {
        None,
        ToLower,
        ToLowerInvariant,
        ToUpper,
        ToUpperInvariant,

    }
}
