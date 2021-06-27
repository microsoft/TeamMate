// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Windows.Media;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    public class BrushLuminosityConverter : OneWayConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = value as SolidColorBrush;
            if (brush != null)
            {
                var percent = System.Convert.ToSingle(parameter);
                if (percent != 0)
                {
                    Color newColor;
                    var hlsColor = new HLSColor(brush.Color);
                    if (percent > 0)
                    {
                        newColor = hlsColor.Lighter(percent);
                    }
                    else
                    {
                        newColor = hlsColor.Darker(-percent);
                    }

                    return new SolidColorBrush(newColor);
                }
            }

            return value;
        }
    }
}
