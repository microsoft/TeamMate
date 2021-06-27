using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// Provides shared instances of global converters.
    /// </summary>
    public static class Converters
    {
        static Converters()
        {
            Boolean = new BooleanConverter();

            var ib = new BooleanConverter();
            ib.Inverse = true;
            InverseBoolean = ib;

            Visibility = new VisibilityConverter();

            var inverse = new VisibilityConverter();
            inverse.Inverse = true;
            InverseVisibility = inverse;

            var vh = new VisibilityConverter();
            vh.WhenInvisible = System.Windows.Visibility.Hidden;
            VisibilityHidden = vh;

            var vhi = new VisibilityConverter();
            vhi.Inverse = true;
            vhi.WhenInvisible = System.Windows.Visibility.Hidden;
            InverseVisibilityHidden = vhi;

            ToLower = new StringConverter(StringConverterMode.ToLower);
            ToLowerInvariant = new StringConverter(StringConverterMode.ToLowerInvariant);
            ToUpper = new StringConverter(StringConverterMode.ToUpper);
            ToUpperInvariant = new StringConverter(StringConverterMode.ToUpperInvariant);

            Percentage = new PercentageConverter();

            Enum = new EnumDisplayStringConverter();

            BrushLuminosity = new BrushLuminosityConverter();
        }

        /// <summary>
        /// Gets a boolean converter.
        /// </summary>
        public static IValueConverter Boolean { get; private set; }

        /// <summary>
        /// Gets an inverse boolean converter.
        /// </summary>
        public static IValueConverter InverseBoolean { get; private set; }

        /// <summary>
        /// Gets a visibility converter.
        /// </summary>
        public static IValueConverter Visibility { get; private set; }

        /// <summary>
        /// Gets an inverse visibility converter.
        /// </summary>
        public static IValueConverter InverseVisibility { get; private set; }

        /// <summary>
        /// Gets a visibility converter whose invisible state is hidden (instead of collapsed).
        /// </summary>
        public static IValueConverter VisibilityHidden { get; private set; }

        /// <summary>
        /// Gets an inverse visibility converter whose invisible state is hidden (instead of collapsed).
        /// </summary>
        public static IValueConverter InverseVisibilityHidden { get; private set; }

        /// <summary>
        /// Gets a string converter that converts to lower case.
        /// </summary>
        public static IValueConverter ToLower { get; private set; }

        /// <summary>
        /// Gets a string converter that converts to invariant lower case.
        /// </summary>
        public static IValueConverter ToLowerInvariant { get; private set; }

        /// <summary>
        /// Gets a string converter that converts to upper case.
        /// </summary>
        public static IValueConverter ToUpper { get; private set; }

        /// <summary>
        /// Gets a string converter that converts to invariant upper case.
        /// </summary>
        public static IValueConverter ToUpperInvariant { get; private set; }

        /// <summary>
        /// Gets a percentage converter.
        /// </summary>
        public static IValueConverter Percentage { get; private set; }

        /// <summary>
        /// Gets an enum converter.
        /// </summary>
        public static IValueConverter Enum { get; private set; }

        public static BrushLuminosityConverter BrushLuminosity { get; private set; }
    }
}
