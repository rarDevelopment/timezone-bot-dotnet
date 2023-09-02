﻿using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using MediatR;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Notifications;

namespace TimeZoneBot.EventHandlers;

public class ReactionAddedNotificationHandler : INotificationHandler<ReactionAddedNotification>
{
    private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
    private readonly IConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly ILogger<DiscordBot> _logger;
    private readonly IDiscordFormatter _discordFormatter;

    public ReactionAddedNotificationHandler(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IConfigurationBusinessLayer configurationBusinessLayer,
        ILogger<DiscordBot> logger,
        IDiscordFormatter discordFormatter)
    {
        _timeZoneBusinessLayer = timeZoneBusinessLayer;
        _configurationBusinessLayer = configurationBusinessLayer;
        _logger = logger;
        _discordFormatter = discordFormatter;
    }
    public Task Handle(ReactionAddedNotification notification, CancellationToken cancellationToken)
    {
        _ = Task.Run(async () =>
        {
            var reaction = notification.Reaction;

            if (!Equals(reaction.Emote, TimeHelpers.GetTimeButtonEmote()))
            {
                return Task.CompletedTask;
            }

            if (notification.Reaction.Channel is not IGuildChannel guildChannel)
            {
                return Task.CompletedTask;
            }

            var config = await _configurationBusinessLayer.GetConfiguration(guildChannel.Guild);
            if (!config.EnableReactions)
            {
                return Task.CompletedTask;
            }

            var regex = new Regex(TimeHelpers.TimeRegexPattern, RegexOptions.IgnoreCase);

            var message = await notification.Message.GetOrDownloadAsync();

            var reactingUser = await reaction.Channel.GetUserAsync(reaction.UserId);

            if (reactingUser.IsBot)
            {
                return Task.CompletedTask;
            }

            var matches = regex.Matches(message.Content);

            var timeMessages = new List<string>();

            try
            {
                foreach (Match match in matches)
                {
                    var personSayingTimeId = message.Author.Id.ToString();
                    var personReactingId = reaction.UserId.ToString();
                    // Note: this might seem flipped, because you are reacting on the time a person said, rather than specifying a time yourself
                    var specifiedTime = match.Value.Trim();
                    var timeForPerson = await _timeZoneBusinessLayer.GetSpecificTimeForPerson(personReactingId, personSayingTimeId, specifiedTime);
                    timeMessages.Add(TimeHelpers.BuildSpecificTimeReactionMessage(timeForPerson.TimeOfDay, specifiedTime, message.Author));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReactionAddedNotificationHandler");
                await message.ReplyAsync(embed: _discordFormatter.BuildErrorEmbedWithUserFooter("Error!",
                    "There was an error retrieving the time(s) for that user. Make sure the user has set up their timezone.", reactingUser), allowedMentions: AllowedMentions.None);
                return Task.CompletedTask;
            }

            var messageToSend = string.Join("\n", timeMessages);

            await message.ReplyAsync(embed: _discordFormatter.BuildRegularEmbedWithUserFooter("Time(s) Requested",
                    messageToSend,
                    reactingUser), allowedMentions: AllowedMentions.None);

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}