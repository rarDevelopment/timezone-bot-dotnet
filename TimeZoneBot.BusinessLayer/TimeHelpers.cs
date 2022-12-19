using System.Globalization;
using NodaTime;

namespace TimeZoneBot.BusinessLayer
{
    public static class TimeHelpers
    {
        public const string TimeFormat = "h:mm tt";
        private const string DayFormat = "dddd, dd MMMM";

        public const string AM = "AM";
        public const string PM = "PM";

        public static string FormatTime(LocalTime time)
        {
            return time.ToString(TimeFormat, new DateTimeFormatInfo());
        }

        public static string FormatDay(ZonedDateTime time)
        {
            return time.ToString(DayFormat, new DateTimeFormatInfo());
        }

        public static string GetEmojiForTime(LocalTime time)
        {
            var formattedTime = FormatTime(time);
            const string replaceText = "{{REPLACE}}";
            var emoji = $":clock{replaceText}:";
            var timeSplit = formattedTime.Split(" ")[0].Split(":"); //first split on space is to remove a.m. or p.m.
            var minutes = Convert.ToInt32(timeSplit[1]);
            emoji = emoji.Replace(replaceText, minutes >= 30
                ? $"{timeSplit[0]}30"
                : $"{timeSplit[0]}");
            return emoji;
        }
    }
}
