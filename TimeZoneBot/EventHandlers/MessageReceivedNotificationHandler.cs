using System.Text.RegularExpressions;
using MediatR;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Notifications;

namespace TimeZoneBot.EventHandlers;

public class MessageReceivedNotificationHandler
    (IConfigurationBusinessLayer configurationBusinessLayer) : INotificationHandler<MessageReceivedNotification>
{
    public Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var message = notification.Message;
            
            if (message.Author.IsBot)
            {
                return Task.CompletedTask;
            }

            var regex = new Regex(TimeHelpers.TimeRegexPattern, RegexOptions.IgnoreCase);
            var match = regex.IsMatch(message.Content);

            if (!match)
            {
                return Task.CompletedTask;
            }

            if (notification.Message.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await configurationBusinessLayer.GetConfiguration(guildChannel.Guild);
            if (!config.EnableReactions)
            {
                return Task.CompletedTask;
            }

            var emote = TimeHelpers.GetTimeButtonEmote();
            await message.AddReactionAsync(emote);

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}