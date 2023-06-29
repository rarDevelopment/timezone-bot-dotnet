using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class TimeAllCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    public TimeAllCommand(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _timeZoneBusinessLayer = timeZoneBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    [SlashCommand("time-all", "Get the current time for the all users in their respective time zones.")]
    public async Task TimeAllSlashCommand(
        [Summary("Time", "The specific time in your time zone for which you'd like to see everyone's times in their time zone.")] string? specifiedTime = null
    )
    {
        var members = Context.Guild.Users.Where(u => u.GetPermissions(Context.Channel as IGuildChannel).ViewChannel && !u.IsBot).ToList();

        await DeferAsync();

        Dictionary<IUser, ZonedDateTime> userTimes = new();

        foreach (var user in members)
        {
            try
            {
                var requesterUserId = Context.User.Id.ToString();
                var targetUserId = user.Id.ToString();

                var time = !string.IsNullOrEmpty(specifiedTime)
                    ? await _timeZoneBusinessLayer.GetSpecificTimeForPerson(targetUserId, requesterUserId, specifiedTime)
                    : await _timeZoneBusinessLayer.GetTimeForPerson(targetUserId);
                if (time == null)
                {
                    continue;
                }
                userTimes.Add(user, time.Value);
            }
            catch (PersonNotFoundException) { }
            catch (TimeZoneNotFoundException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in TimeAllSlashCommand");
            }
        }

        var messagesToSend = userTimes.OrderBy(t => t.Value.Offset)
            .Select(userTime => BuildTimeMessage(userTime.Value, userTime.Key));
        var messageToSend = string.Join("\n", messagesToSend);

        if (!string.IsNullOrEmpty(messageToSend))
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Current Time for Everyone in this Channel",
            messageToSend, Context.User));
        }
    }

    private static string BuildTimeMessage(ZonedDateTime time, IUser user)
    {
        var emoji = $"{TimeHelpers.GetEmojiForTime(time.TimeOfDay)}";
        return $"{emoji} **{user.GetNameToDisplay()}**'s current time is **{TimeHelpers.FormatTime(time.TimeOfDay)}** ({TimeHelpers.FormatDay(time)})";
    }
}