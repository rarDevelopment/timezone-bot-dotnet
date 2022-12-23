using Discord;
using TimeZoneBot.Models;

namespace TimeZoneBot.BusinessLayer.Interfaces
{
    public interface IConfigurationBusinessLayer
    {
        Task<Configuration> GetConfiguration(IGuild guild);
        Task<bool> SetReactionsEnabled(IGuild guild, bool isEnabled);
    }
}
