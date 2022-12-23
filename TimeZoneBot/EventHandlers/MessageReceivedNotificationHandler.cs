using System.Text.RegularExpressions;
using MediatR;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Notifications;

namespace TimeZoneBot.EventHandlers;

public class MessageReceivedNotificationHandler : INotificationHandler<MessageReceivedNotification>
{
    private readonly IConfigurationBusinessLayer _configurationBusinessLayer;

    public MessageReceivedNotificationHandler(IConfigurationBusinessLayer configurationBusinessLayer)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
    }

    public Task Handle(MessageReceivedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var message = notification.Message;

            var regex = new Regex(TimeHelpers.TimeRegexPattern);
            var match = regex.IsMatch(message.Content);

            // TODO: check for configuration options for enabling the reaction settings
            if (!match)
            {
                return Task.CompletedTask;
            }

            if (notification.Message.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await _configurationBusinessLayer.GetConfiguration(guildChannel.Guild);
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