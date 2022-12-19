using NodaTime;
using System.Globalization;

namespace TimeZoneBot.BusinessLayer
{
    public class BirthdayHelpers
    {
        private const string BirthdayFormat = "dddd, dd MMMM";

        public static string FormatBirthday(LocalDate birthday)
        {
            return birthday.ToString(BirthdayFormat, new DateTimeFormatInfo());
        }
    }
}
