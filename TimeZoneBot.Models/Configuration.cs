namespace TimeZoneBot.Models;

public class Configuration
{
    public string GuildId { get; set; }
    public string GuildName { get; set; }
    public bool EnableReactions { get; set; }
    public string? DefaultTimeZone { get; set; }
    public bool EnableBirthdayAnnouncements { get; set; }
}