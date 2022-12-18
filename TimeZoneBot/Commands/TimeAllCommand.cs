using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
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
    public async Task TimeAllSlashCommand()
    {
        var members = Context.Guild.Users.Where(u => u.GetPermissions(Context.Channel as IGuildChannel).ViewChannel && !u.IsBot);

        await DeferAsync();

        Dictionary<IUser, ZonedDateTime> userTimes = new();

        foreach (var user in members)
        {
            try
            {
                var time = await _timeZoneBusinessLayer.GetTimeForPerson(user.Id);
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
                _logger.LogError(ex, "Unhandled error in TimeSlashCommand");
            }
        }

        var messagesToSend = userTimes.OrderBy(t => t.Value.ToInstant()).Select(userTime => BuildTimeMessage(userTime.Value, userTime.Key));
        var messageToSend = string.Join("\n", messagesToSend);

        if (!string.IsNullOrEmpty(messageToSend))
        {
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Current Time for Everyone in this Channel",
            messageToSend, Context.User));
        }
    }

    private static string BuildTimeMessage(ZonedDateTime time, IUser user)
    {
        var emoji = $"{TimeHelpers.GetEmojiForTime(time)}";
        return $"{emoji} **{user.Username}**'s current time is **{TimeHelpers.FormatTime(time)}** ({TimeHelpers.FormatDay(time)})";
        // {time.ToString("%x", new DateTimeFormatInfo())}
    }
}