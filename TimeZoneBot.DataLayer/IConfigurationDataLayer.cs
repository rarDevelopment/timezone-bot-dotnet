using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer;

public interface IConfigurationDataLayer
{
    Task<Configuration> GetConfigurationForGuild(string guildId, string guildName);
    Task<bool> SetReactionsEnabled(string guildId, bool isEnabled);
    Task<bool> SetDefaultTimeZone(string guildId, string timeZone);
    Task<bool> SetBirthdayAnnouncementsEnabled(string guildId, bool isEnabled);
}