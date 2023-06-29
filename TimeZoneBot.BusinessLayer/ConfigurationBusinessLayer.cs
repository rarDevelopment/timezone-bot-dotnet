using Discord;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.DataLayer;
using TimeZoneBot.Models;

namespace TimeZoneBot.BusinessLayer;

public class ConfigurationBusinessLayer : IConfigurationBusinessLayer
{
    private readonly IConfigurationDataLayer _configurationDataLayer;

    public ConfigurationBusinessLayer(IConfigurationDataLayer configurationDataLayer)
    {
        _configurationDataLayer = configurationDataLayer;
    }

    public async Task<Configuration> GetConfiguration(IGuild guild)
    {
        return await _configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
    }

    public async Task<bool> SetReactionsEnabled(IGuild guild, bool isEnabled)
    {
        Configuration? config = await _configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _configurationDataLayer.SetReactionsEnabled(guild.Id.ToString(), isEnabled);
        }
        return false;
    }

    public async Task<bool> SetBirthdayAnnouncementsEnabled(IGuild guild, bool isEnabled)
    {
        Configuration? config = await _configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _configurationDataLayer.SetBirthdayAnnouncementsEnabled(guild.Id.ToString(), isEnabled);
        }
        return false;
    }

    public async Task<bool> SetDefaultTimeZone(IGuild guild, string timeZone)
    {
        Configuration? config = await _configurationDataLayer.GetConfigurationForGuild(guild.Id.ToString(), guild.Name);
        if (config != null)
        {
            return await _configurationDataLayer.SetDefaultTimeZone(guild.Id.ToString(), timeZone);
        }
        return false;
    }
}