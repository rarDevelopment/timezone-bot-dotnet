using NodaTime;

namespace TimeZoneBot.Models
{
    public class Person
    {
        public Person(ulong snowflake, string? timeZone, LocalDate? birthday)
        {
            Snowflake = snowflake;
            TimeZone = timeZone;
            Birthday = birthday;
        }
        public ulong Snowflake { get; set; }
        public string? TimeZone { get; set; }
        public LocalDate? Birthday { get; set; }
    }
}
