using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using MediatR;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Notifications;

namespace TimeZoneBot.EventHandlers;

public class ReactionAddedNotificationHandler : INotificationHandler<ReactionAddedNotification>
{
    private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
    private readonly ILogger<DiscordBot> _logger;
    private readonly IDiscordFormatter _discordFormatter;

    public ReactionAddedNotificationHandler(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        ILogger<DiscordBot> logger,
        IDiscordFormatter discordFormatter)
    {
        _timeZoneBusinessLayer = timeZoneBusinessLayer;
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

            var regex = new Regex(TimeHelpers.TimeRegexPattern);

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
                    var timeForPerson =
                        await _timeZoneBusinessLayer.GetSpecificTimeForPerson(message.Author.Id, reaction.UserId,
                            match.Value);
                    timeMessages.Add(TimeHelpers.BuildSpecificTimeMessage(timeForPerson.TimeOfDay, match.Value, message.Author));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReactionAddedNotificationHandler");
                await message.ReplyAsync(embed: _discordFormatter.BuildErrorEmbed("Error!",
                    "There was an error retrieving the time(s) for that user. Make sure the user has set up their timezone."), allowedMentions: AllowedMentions.None);
            }

            var messageToSend = string.Join("\n", timeMessages);

            await message.ReplyAsync(embed: _discordFormatter.BuildRegularEmbed("Time(s) Requested",
                    messageToSend,
                    reactingUser), allowedMentions: AllowedMentions.None);

            return Task.CompletedTask;
        }, cancellationToken);
        return Task.CompletedTask;
    }
}