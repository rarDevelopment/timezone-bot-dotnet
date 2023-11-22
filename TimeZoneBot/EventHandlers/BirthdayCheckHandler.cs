using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.EventHandlers;

public class BirthdayCheckHandler(DiscordSocketClient client,
    ITimeZoneBusinessLayer timeZoneBusinessLayer,
    IBirthdayBusinessLayer birthdayBusinessLayer,
    IConfigurationBusinessLayer configurationBusinessLayer,
    DiscordSettings discordSettings,
    ILogger<BirthdayCheckHandler> logger)
{
    public async Task HandleBirthdayCheck()
    {
        var guilds = client.Guilds;
        foreach (var guild in guilds)
        {
            try
            {
                await SendMessagesForBirthdaysInGuild(guild);
            }
            catch (Exception ex)
            {
                logger.LogError($"Failed to post to guild {guild.Name} (id: {guild.Id}) - {ex.Message}");
            }
        }
    }

    private async Task SendMessagesForBirthdaysInGuild(SocketGuild guild)
    {
        var configuration = await configurationBusinessLayer.GetConfiguration(guild);
        if (!configuration.EnableBirthdayAnnouncements)
        {
            return;
        }

        var membersToCheck = guild.Users;
        foreach (var member in membersToCheck)
        {
            var id = member.Id.ToString();
            var birthdayToCheck = await birthdayBusinessLayer.GetBirthdayForPerson(id);
            if (birthdayToCheck == null)
            {
                continue;
            }

            ZonedDateTime? timeForUser;
            try
            {
                timeForUser = await timeZoneBusinessLayer.GetTimeForPerson(id);
            }
            catch (NoTimeZoneException)
            {
                var defaultTimeZone = configuration.DefaultTimeZone;
                if (defaultTimeZone == null)
                {
                    continue;
                }
                timeForUser = timeZoneBusinessLayer.GetTimeInTimeZone(defaultTimeZone);
            }

            if (timeForUser == null)
            {
                continue;
            }

            if (timeForUser.Value.Day == birthdayToCheck.Value.Day
                && timeForUser.Value.Month == birthdayToCheck.Value.Month
                && timeForUser.Value.Hour == discordSettings.HourForBirthdayAnnouncements)
            {
                await guild.SystemChannel.SendMessageAsync(
                    $"Today is {member.Mention}'s birthday! Happy birthday {member.Mention}!! 🎂");
            }
        }
    }
}