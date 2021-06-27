using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// A converter that converts input items to the matching values in a dictionary.
    /// </summary>
    public class StaticMapConverter : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StaticMapConverter"/> class.
        /// </summary>
        /// <param name="map">The map.</param>
        public StaticMapConverter(IDictionary<object, object> map)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        public IDictionary<object, object> Map { get; private set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object result;
            Map.TryGetValue(value, out result);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var item in Map)
            {
                if (Object.Equals(value, item.Value))
                {
                    return item.Key;
                }
            }

            return null;
        }
    }
}
