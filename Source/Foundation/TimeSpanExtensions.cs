using Microsoft.Internal.Tools.TeamMate.Foundation.Resources;
using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Internal.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides extension methods for time spans.
    /// </summary>
    public static class TimeSpanExtensions
    {
        private const double SecondsInMinute = 60;
        private const double SecondsInHour = SecondsInMinute * 60;
        private const double SecondsInDay = SecondsInHour * 24;
        private const double SecondsInWeek = SecondsInDay * 7;
        private const double SecondsInMonth = (SecondsInDay * 365) / 12;
        private const double SecondsInYear = SecondsInDay * 365;

        private static readonly ElapsedTimeFormatSpec[] ElapsedTimeFormatSpecs = new ElapsedTimeFormatSpec[] 
        { 
            new ElapsedTimeFormatSpec(SecondsInMinute, ResourceStrings.ElapsedTimeLessThanMinute, 0),
            new ElapsedTimeFormatSpec(SecondsInMinute * 1.5, ResourceStrings.ElapsedTimeAMinute, 0),
            new ElapsedTimeFormatSpec(SecondsInHour, String.Format(ResourceStrings.ElapsedTimeMinutes, "{0}"), SecondsInMinute),
            new ElapsedTimeFormatSpec(SecondsInHour * 1.5, ResourceStrings.ElapsedTimeAnHour, 0),
            new ElapsedTimeFormatSpec(SecondsInDay, String.Format(ResourceStrings.ElapsedTimeHours, "{0}"), SecondsInHour),            
            new ElapsedTimeFormatSpec(SecondsInDay * 1.5, ResourceStrings.ElapsedTimeADay, 0),
            new ElapsedTimeFormatSpec(SecondsInWeek, String.Format(ResourceStrings.ElapsedTimeDays, "{0}"), SecondsInDay),
            new ElapsedTimeFormatSpec(SecondsInWeek * 1.5, ResourceStrings.ElapsedTimeAWeek, 0),
            new ElapsedTimeFormatSpec(SecondsInMonth, String.Format(ResourceStrings.ElapsedTimeWeeks, "{0}"), SecondsInWeek),
            new ElapsedTimeFormatSpec(SecondsInMonth * 1.5, ResourceStrings.ElapsedTimeAMonth, 0),
            new ElapsedTimeFormatSpec(SecondsInYear, String.Format(ResourceStrings.ElapsedTimeMonths, "{0}"), SecondsInMonth),
            new ElapsedTimeFormatSpec(SecondsInYear * 1.5, ResourceStrings.ElapsedTimeAYear, 0),
            new ElapsedTimeFormatSpec(double.MaxValue, String.Format(ResourceStrings.ElapsedTimeYears, "{0}"), SecondsInYear)
        };

        /// <summary>
        /// Formats a time span in a friendly string (e.g. "less than a minute", "3 hours", "4 months").
        /// </summary>
        public static string ToFriendlyString(this TimeSpan timeSpan)
        {
            // TODO: Assert is positive difference or handle future date?

            double seconds = timeSpan.TotalSeconds;

            for (int i = 0; i < ElapsedTimeFormatSpecs.Length; i++)
            {
                var step = ElapsedTimeFormatSpecs[i];
                double timeDelta = seconds;
                double limit = step.Limit;

                // If there is a divisible argument, divide both the time and the limit to avoid rounding
                // issues when doing the comparison (otherwise you can get strings like '60 minutes ago' instead of
                // 'one hour ago' due to rounding issues)
                if (step.DivisibleArgument > 0)
                {
                    timeDelta = Math.Round(timeDelta / step.DivisibleArgument);
                    limit = Math.Round(limit / step.DivisibleArgument);
                }

                if (timeDelta < limit || i == ElapsedTimeFormatSpecs.Length - 1)
                {
                    if (step.DivisibleArgument > 0)
                    {
                        return String.Format(CultureInfo.CurrentCulture, step.Format, timeDelta);
                    }
                    else
                    {
                        return step.Format;
                    }
                }
            }

            Debug.Fail("Should have never made it here!");
            return String.Empty;
        }

        /// <summary>
        /// Formats a time span to the shortest time specification (e.g. 00:12, or 1:00:04, or 5.03:00:01).
        /// </summary>
        /// <param name="time">The time.</param>
        public static string ToShortestSecondsString(this TimeSpan time)
        {
            if (time.Days > 0)
            {
                return time.ToString(@"d\.hh\:mm\:ss");
            }
            else if (time.TotalHours >= 1)
            {
                return time.ToString(@"h\:mm\:ss");
            }
            else
            {
                return time.ToString(@"mm\:ss");
            }
        }

        /// <summary>
        /// Helper class to define format specifications.
        /// </summary>
        private class ElapsedTimeFormatSpec
        {
            public ElapsedTimeFormatSpec(double limit, string format, double divisibleArgument)
            {
                this.Limit = limit;
                this.Format = format;
                this.DivisibleArgument = divisibleArgument;
            }

            public double Limit { get; private set; }

            public string Format { get; private set; }

            public double DivisibleArgument { get; private set; }
        }
    }
}
