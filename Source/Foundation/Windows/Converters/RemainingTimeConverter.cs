// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Text;

namespace Microsoft.Tools.TeamMate.Foundation.Windows
{
    /// <summary>
    /// A converter that converts a time span to remaining time.
    /// </summary>
    public class RemainingTimeConverter : OneWayConverterBase
    {
        /// <summary>
        /// Converts an input nullable time span to a friendly string.
        /// </summary>
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            TimeSpan? remainingTime = value as TimeSpan?;
            return (remainingTime != null) ? ToRemainingTimeString(remainingTime.Value) : null;
        }

        /// <summary>
        /// Formats a timespan as a remaining time message.
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>The formatted string.</returns>
        private static string ToRemainingTimeString(TimeSpan timeSpan)
        {
            string result;
            if (timeSpan.TotalSeconds < 1)
            {
                result = "Almost Done...";
            }
            else
            {
                result = String.Format("About {0} remaining", ToFriendlyString(timeSpan));
            }

            return result;
        }

        /// <summary>
        /// Formats a timespan as a long human-readable string (e.g. 3 Hours 1 Minute 5 Seconds).
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        /// <returns>The formatted string.</returns>
        private static string ToFriendlyString(TimeSpan timeSpan)
        {
            StringBuilder sb = new StringBuilder();

            if (timeSpan.Hours > 0)
            {
                AppendTimeComponent(sb, timeSpan.Hours, "Hours", "Hour");
            }

            if (timeSpan.Minutes > 0)
            {
                AppendTimeComponent(sb, timeSpan.Minutes, "Minutes", "Minute");
            }

            if (timeSpan.Seconds > 0 || timeSpan.TotalSeconds < 1)
            {
                AppendTimeComponent(sb, timeSpan.Seconds, "Seconds", "Second");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Appends a time component to a string builder.
        /// </summary>
        /// <param name="sb">The string builder.</param>
        /// <param name="count">The unit count.</param>
        /// <param name="unitPlural">The unit plural word.</param>
        /// <param name="unitSingular">The unit singular word.</param>
        private static void AppendTimeComponent(StringBuilder sb, int count, string unitPlural, string unitSingular)
        {
            if (sb.Length > 0)
            {
                sb.Append(' ');
            }

            string word = (count == 1) ? unitSingular : unitPlural;
            sb.AppendFormat("{0} {1}", count, word);
        }
    }
}
