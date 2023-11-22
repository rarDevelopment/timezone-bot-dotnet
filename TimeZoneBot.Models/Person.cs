using NodaTime;

namespace TimeZoneBot.Models;

public class Person(string userId, string? timeZone, LocalDate? birthday)
{
    public string UserId { get; set; } = userId;
    public string? TimeZone { get; set; } = timeZone;
    public LocalDate? Birthday { get; set; } = birthday;
}