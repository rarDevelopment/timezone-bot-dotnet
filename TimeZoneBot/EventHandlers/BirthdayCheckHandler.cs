using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.EventHandlers;

public class BirthdayCheckHandler
{
    private readonly DiscordSocketClient _client;
    private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
    private readonly IBirthdayBusinessLayer _birthdayBusinessLayer;
    private readonly IConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly DiscordSettings _discordSettings;
    private readonly ILogger<BirthdayCheckHandler> _logger;

    public BirthdayCheckHandler(DiscordSocketClient client,
        ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IBirthdayBusinessLayer birthdayBusinessLayer,
        IConfigurationBusinessLayer configurationBusinessLayer,
        DiscordSettings discordSettings,
        ILogger<BirthdayCheckHandler> logger)
    {
        _client = client;
        _timeZoneBusinessLayer = timeZoneBusinessLayer;
        _birthdayBusinessLayer = birthdayBusinessLayer;
        _configurationBusinessLayer = configurationBusinessLayer;
        _discordSettings = discordSettings;
        _logger = logger;
    }

    public async Task HandleBirthdayCheck()
    {
        var guilds = _client.Guilds;
        foreach (var guild in guilds)
        {
            try
            {
                await SendMessagesForBirthdaysInGuild(guild);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to post to guild {guild.Name} (id: {guild.Id}) - {ex.Message}");
            }
        }
    }

    private async Task SendMessagesForBirthdaysInGuild(SocketGuild guild)
    {
        var configuration = await _configurationBusinessLayer.GetConfiguration(guild);
        if (!configuration.EnableBirthdayAnnouncements)
        {
            return;
        }

        var membersToCheck = guild.Users;
        foreach (var member in membersToCheck)
        {
            var id = member.Id.ToString();
            var birthdayToCheck = await _birthdayBusinessLayer.GetBirthdayForPerson(id);
            if (birthdayToCheck == null)
            {
                continue;
            }

            ZonedDateTime? timeForUser;
            try
            {
                timeForUser = await _timeZoneBusinessLayer.GetTimeForPerson(id);
            }
            catch (NoTimeZoneException)
            {
                var defaultTimeZone = configuration.DefaultTimeZone;
                if (defaultTimeZone == null)
                {
                    continue;
                }
                timeForUser = _timeZoneBusinessLayer.GetTimeInTimeZone(defaultTimeZone);
            }

            if (timeForUser == null)
            {
                continue;
            }

            if (timeForUser.Value.Day == birthdayToCheck.Value.Day
                && timeForUser.Value.Month == birthdayToCheck.Value.Month
                && timeForUser.Value.Hour == _discordSettings.HourForBirthdayAnnouncements)
            {
                await guild.SystemChannel.SendMessageAsync(
                    $"Today is {member.Mention}'s birthday! Happy birthday {member.Mention}!! 🎂");
            }
        }
    }
}