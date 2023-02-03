using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models;

namespace TimeZoneBot.EventHandlers
{
    public class BirthdayCheckHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
        private readonly IBirthdayBusinessLayer _birthdayBusinessLayer;
        private readonly DiscordSettings _discordSettings;

        public BirthdayCheckHandler(DiscordSocketClient client,
            ITimeZoneBusinessLayer timeZoneBusinessLayer,
            IBirthdayBusinessLayer birthdayBusinessLayer,
            DiscordSettings discordSettings)
        {
            _client = client;
            _timeZoneBusinessLayer = timeZoneBusinessLayer;
            _birthdayBusinessLayer = birthdayBusinessLayer;
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
                var timeForUser = await _timeZoneBusinessLayer.GetTimeForPerson(member.Id);
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
