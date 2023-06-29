using NodaTime;

namespace TimeZoneBot.Models;

public class Person
{
    public Person(string userId, string? timeZone, LocalDate? birthday)
    {
        UserId = userId;
        TimeZone = timeZone;
        Birthday = birthday;
    }
    public string UserId { get; set; }
    public string? TimeZone { get; set; }
    public LocalDate? Birthday { get; set; }
}