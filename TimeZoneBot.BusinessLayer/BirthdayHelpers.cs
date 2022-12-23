using NodaTime;
using System.Globalization;

namespace TimeZoneBot.BusinessLayer
{
    public class BirthdayHelpers
    {
        private const string BirthdayFormat = "MMMM dd, yyyy";
        private const string BirthdayFormatWithDay = "MMMM dd, yyyy (dddd)";
        private const string SortableBirthdayFormat = "yyyy-MM-dd";

        public static string FormatBirthday(LocalDate birthday, bool includeDay = false)
        {
            return includeDay
                ? birthday.ToString(BirthdayFormatWithDay, new DateTimeFormatInfo())
                : birthday.ToString(BirthdayFormat, new DateTimeFormatInfo());
        }

        public static string FormatSortableBirthday(LocalDate birthday)
        {
            return birthday.ToString(SortableBirthdayFormat, new DateTimeFormatInfo());
        }
    }
}
