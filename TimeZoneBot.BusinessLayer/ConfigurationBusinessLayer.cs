using Discord;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.DataLayer;
using TimeZoneBot.Models;

namespace TimeZoneBot.BusinessLayer;

public class ConfigurationBusinessLayer(IConfigurationDataLayer configurationDataLayer) : IConfigurationBusinessLayer
{
    public async Task<Configuration> GetConfiguration(IGuild guild)
    {
        return await configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
    }

    public async Task<bool> SetReactionsEnabled(IGuild guild, bool isEnabled)
    {
        Configuration? config = await configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await configurationDataLayer.SetReactionsEnabled(guild.Id.ToString(), isEnabled);
        }
        return false;
    }

    public async Task<bool> SetBirthdayAnnouncementsEnabled(IGuild guild, bool isEnabled)
    {
        Configuration? config = await configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await configurationDataLayer.SetBirthdayAnnouncementsEnabled(guild.Id.ToString(), isEnabled);
        }
        return false;
    }

    public async Task<bool> SetDefaultTimeZone(IGuild guild, string timeZone)
    {
        Configuration? config = await configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await configurationDataLayer.SetDefaultTimeZone(guild.Id.ToString(), timeZone);
        }
        return false;
    }
}