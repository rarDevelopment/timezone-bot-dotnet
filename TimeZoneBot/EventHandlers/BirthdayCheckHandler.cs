using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.EventHandlers
{
    public class BirthdayCheckHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
        private readonly IBirthdayBusinessLayer _birthdayBusinessLayer;
        private readonly IConfigurationBusinessLayer _configurationBusinessLayer;
        private readonly DiscordSettings _discordSettings;

        public BirthdayCheckHandler(DiscordSocketClient client,
            ITimeZoneBusinessLayer timeZoneBusinessLayer,
            IBirthdayBusinessLayer birthdayBusinessLayer,
            IConfigurationBusinessLayer configurationBusinessLayer,
            DiscordSettings discordSettings)
        {
            _client = client;
            _timeZoneBusinessLayer = timeZoneBusinessLayer;
            _birthdayBusinessLayer = birthdayBusinessLayer;
            _configurationBusinessLayer = configurationBusinessLayer;
            _discordSettings = discordSettings;
        }

        public async Task HandleBirthdayCheck()
        {
            var guilds = _client.Guilds;
            foreach (var guild in guilds)
            {
                await SendMessagesForBirthdaysInGuild(guild);
            }
        }

        private async Task SendMessagesForBirthdaysInGuild(SocketGuild guild)
        {
            var membersToCheck = guild.Users;
            foreach (var member in membersToCheck)
            {
                var birthdayToCheck = await _birthdayBusinessLayer.GetBirthdayForPerson(member.Id);
                if (birthdayToCheck == null)
                {
                    continue;
                }

                ZonedDateTime? timeForUser;
                try
                {
                    timeForUser = await _timeZoneBusinessLayer.GetTimeForPerson(member.Id);
                }
                catch (NoTimeZoneException)
                {
                    var configuration = await _configurationBusinessLayer.GetConfiguration(guild);
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
}
