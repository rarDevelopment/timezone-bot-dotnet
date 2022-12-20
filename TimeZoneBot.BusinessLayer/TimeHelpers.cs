using System.Globalization;
using NodaTime;

namespace TimeZoneBot.BusinessLayer
{
    public static class TimeHelpers
    {
        public const string TimeRegexPattern =
            "(([0-1]?[0-9]|2[0-3]):[0-5][0-9]( |)(am|pm|)|([0-1]?[0-9]|2[0-3])( |)(am|pm))";
        public const string TimeFormat = "h:mm tt";
        public const string TimeFormat24Hour = "H:mm";
        private const string DayFormat = "dddd, dd MMMM";

        public static bool HasMeridiem(Meridiem meridiem, string time)
        {
            return time.ToLower().Contains(meridiem.ToString().ToLower());
        }

        public static bool HasAnyMeridiem(string time)
        {
            var allValues = GetAllMeridiems();
            return allValues.Any(meridiem => HasMeridiem(meridiem, time));
        }

        public static Meridiem[] GetAllMeridiems()
        {
            return (Meridiem[])Enum.GetValues(typeof(Meridiem));
        }

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
