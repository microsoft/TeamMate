using Microsoft.Internal.Tools.TeamMate.Foundation.Resources;
using System;
using System.Globalization;

namespace Microsoft.Internal.Tools.TeamMate.Foundation
{
    /// <summary>
    /// Provides utility methods for working with date times.
    /// </summary>
    public static class DateTimeExtensions
    {
        private static string[] Numbers = {
            "Zero", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten"                                                
        };

        /// <summary>
        /// Determines whether the specified date is after another date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="other">The other.</param>
        /// <returns><c>true</c> if a date was after the other date.</returns>
        public static bool IsAfter(this DateTime date, DateTime? other)
        {
            // Very important to make sure both dates are in the same space, otherwise the comparison gives you bogus values
            return other == null || other.Value.ToUniversalTime() < date.ToUniversalTime();
        }

        /// <summary>
        /// Determines whether the specified date is between (inclusive) two dates.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="start">The start date.</param>
        /// <param name="end">The end date.</param>
        /// <returns></returns>
        public static bool IsBetween(this DateTime date, DateTime start, DateTime end)
        {
            return start <= date && date <= end;
        }

        /// <summary>
        /// Returns a string to describe the time interval between this date and the current time.
        /// E.g. "3 minutes", or "11 seconds".
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToFriendlyElapsedTimeString(this DateTime date)
        {
            TimeSpan difference = (DateTime.UtcNow - date.ToUniversalTime());

            // TODO: Assert is positive difference or handle future date?
            return difference.ToFriendlyString();
        }

        /// <summary>
        /// Returns a string to describe the time interval between this date and the current time, with the "ago" word.
        /// E.g. "3 minutes ago", or "11 seconds ago".
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToFriendlyElapsedTimeStringWithAgo(this DateTime date)
        {
            string result = date.ToFriendlyElapsedTimeString();
            return String.Format(ResourceStrings.Ago, result);
        }

        /// <summary>
        /// Returns a short date string that represents this date. The string format depends on the
        /// the time difference with now (e.g. "3 minutes ago", "Today at 3:00PM", "Yesterday at 11:00AM",
        /// or "on Monday 10/16/2014");
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToFriendlyShortDateString(this DateTime date)
        {
            return date.ToFriendlyDateString(false);
        }

        /// <summary>
        /// Returns a date string that represents this date. The string format depends on the
        /// the time difference with now (e.g. "3 minutes ago", "Today at 3:00PM", "Yesterday at 11:00AM",
        /// or "on Monday 10/16/2014");
        /// </summary>
        /// <param name="date">The date.</param>
        public static string ToFriendlyLongDateString(this DateTime date)
        {
            return date.ToFriendlyDateString(true);
        }

        /// <summary>
        /// Returns a string that represents the date in cormparison to the current time, only taking into
        /// account the date component (eg. "Today", "Yesterday", "Monday", "Last Week", "3 Weeks Ago", "Last Month",
        /// "Older", "Newer").
        /// </summary>
        /// <param name="date">The date.</param>
        /// <remarks>
        /// Based on Outlook's date grouping for conversations.
        /// </remarks>
        public static string ToFriendlyDatePeriod(this DateTime date)
        {
            DateTime today = DateTime.Today;

            TimeSpan difference = today - date.ToLocalTime().Date;

            if (difference >= TimeSpan.Zero)
            {
                if (difference.TotalDays < 1)
                {
                    return "Today";
                }

                if (difference.TotalDays < 2)
                {
                    return "Yesterday";
                }

                int weeksDifference = today.WeeksDifference(date);
                if (weeksDifference == 0)
                {
                    // A day in this week
                    return date.ToString("dddd");
                }

                if (weeksDifference > 0 && weeksDifference <= 3 && weeksDifference < Numbers.Length)
                {
                    // A previous week in this month
                    return (weeksDifference == 1) ? "Last Week" : String.Format("{0} Weeks Ago", Numbers[weeksDifference]);
                }

                int monthsDifference = today.MonthsDifference(date);
                if (monthsDifference == 1)
                {
                    return "Last Month";
                }

                return "Older";
            }
            else
            {
                return "Newer";
            }
        }


        /// <summary>
        /// Returns a short date string that represents this date. The string format depends on the
        /// the time difference with now (e.g. "3 minutes ago", "Today at 3:00PM", "Yesterday at 11:00AM",
        /// or "on Monday 10/16/2014");
        /// </summary>
        /// <param name="date">The date.</param>
        private static string ToFriendlyDateString(this DateTime date, bool longDate)
        {
            // Make sure the date is in local time
            date = date.ToLocalTime();
            DateTime now = DateTime.Now;

            TimeSpan timeDifference = (now - date);

            // TODO: Assert is positive difference or handle future date?

            if (timeDifference.TotalHours < 7)
            {
                string result = timeDifference.ToFriendlyString();
                return String.Format(ResourceStrings.Ago, result);
            }
            else if (date.Date == now.Date)
            {
                string time = date.ToShortTimeString();
                return String.Format(ResourceStrings.TimeStampTodayAt, time);
            }
            else if ((date + TimeSpan.FromDays(1)).Date == now.Date)
            {
                string time = date.ToShortTimeString();
                return String.Format(ResourceStrings.TimeStampYesterdayAt, time);
            }
            else
            {
                if (longDate)
                {
                    if (timeDifference.TotalDays <= 5)
                    {
                        string day = date.ToString("dddd", CultureInfo.CurrentCulture);
                        string time = date.ToShortTimeString();
                        return String.Format(ResourceStrings.TimeStampDayAt, day, time);
                    }
                    else
                    {
                        string day = date.ToString("ddd", CultureInfo.CurrentCulture);
                        string time = date.ToString("g", CultureInfo.CurrentCulture);
                        return String.Format(ResourceStrings.TimeStampFullDate, day, time);
                    }
                }
                else
                {
                    string day = date.ToString("ddd", CultureInfo.CurrentCulture);
                    string fullDate = String.Format(ResourceStrings.DayAndDate, day, date.ToShortDateString());
                    return String.Format(ResourceStrings.OnDate, fullDate);
                }
            }
        }

        /// <summary>
        /// Gets the number of months between two dates.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="otherDate">The other date.</param>
        /// <returns>The number of months between the two dates (negative if otherDate is later than this date).</returns>
        private static int MonthsDifference(this DateTime date, DateTime otherDate)
        {
            var diff = (date.Year - otherDate.Year) * 12 + (date.Month - otherDate.Month);
            return diff;
        }

        /// <summary>
        /// Gets the number of weeks between two dates.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="otherDate">The other date.</param>
        /// <returns>The number of weeks between the two dates (negative if otherDate is later than this date).</returns>
        private static int WeeksDifference(this DateTime date, DateTime otherDate)
        {
            var culture = CultureInfo.CurrentCulture;
            var calendarWeekRule = CalendarWeekRule.FirstDay;
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            int week1 = culture.Calendar.GetWeekOfYear(date, calendarWeekRule, firstDayOfWeek);
            int week2 = culture.Calendar.GetWeekOfYear(otherDate, calendarWeekRule, firstDayOfWeek);

            var diff = (date.Year - otherDate.Year) * 52 + (week1 - week2);
            return diff;
        }
    }
}
