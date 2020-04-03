using System;

namespace DotaDb.Data.Utilities
{
    public static class DateTimeOffsetExtensions
    {
        public static string GetTimeAgo(this DateTimeOffset? dateReviewCreated)
        {
            if (!dateReviewCreated.HasValue)
            {
                return "Unknown";
            }

            string timeAgoValue = String.Empty;
            string timeAgoSpecifier = String.Empty;

            double totalMinutesAgo = Math.Floor(DateTime.UtcNow.Subtract(dateReviewCreated.Value.UtcDateTime).TotalMinutes);

            int hoursInDay = 24;
            int minutesInHour = 60;
            int minutesInDay = 60 * 24;
            int minutesInWeek = minutesInDay * 7;

            if (totalMinutesAgo < 1)
            {
                timeAgoValue = "less than 1";
                timeAgoSpecifier = "minute ago";
            }
            else if (totalMinutesAgo == 1)
            {
                timeAgoValue = totalMinutesAgo.ToString("F0");
                timeAgoSpecifier = "minute ago";
            }
            else if (totalMinutesAgo > 1 && totalMinutesAgo < minutesInHour)
            {
                timeAgoValue = totalMinutesAgo.ToString("F0");
                timeAgoSpecifier = "minutes ago";
            }
            else if (totalMinutesAgo == minutesInHour)
            {
                timeAgoValue = "1";
                timeAgoSpecifier = "hour ago";
            }
            else if (totalMinutesAgo > minutesInHour && totalMinutesAgo < minutesInDay) // between 1 hour and 24 hour
            {
                double timeAgoNumber = Math.Round((double)totalMinutesAgo / (double)minutesInHour);
                timeAgoValue = timeAgoNumber.ToString("F0");

                if (timeAgoNumber == 1)
                {
                    timeAgoSpecifier = "hour ago";
                }
                else
                {
                    timeAgoSpecifier = "hours ago";
                }
            }
            else if (totalMinutesAgo == minutesInDay) // exactly 24 hours (1 day)
            {
                timeAgoValue = "1";
                timeAgoSpecifier = "day ago";
            }
            else if (totalMinutesAgo > minutesInDay && totalMinutesAgo < minutesInWeek) // greater than 1 day
            {
                double timeAgoNumber = Math.Round((double)totalMinutesAgo / (double)minutesInHour / (double)hoursInDay);
                timeAgoValue = timeAgoNumber.ToString("F0");

                if (timeAgoNumber == 1)
                {
                    timeAgoSpecifier = "day ago";
                }
                else
                {
                    timeAgoSpecifier = "days ago";
                }
            }
            else if (totalMinutesAgo >= minutesInWeek)
            {
                timeAgoValue = dateReviewCreated.Value.UtcDateTime.ToString("m");
            }

            string timeAgoDisplay = String.Format("{0} {1}", timeAgoValue, timeAgoSpecifier);
            return timeAgoDisplay;
        }
    }
}