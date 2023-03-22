using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer;

public interface IConfigurationDataLayer
{
    Task<Configuration> GetConfigurationForGuild(ulong guildId, string guildName);
    Task<bool> SetReactionsEnabled(ulong guildId, bool isEnabled);
    Task<bool> SetDefaultTimeZone(ulong guildId, string timeZone);
}