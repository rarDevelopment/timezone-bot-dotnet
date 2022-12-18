using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class TimeCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    public TimeCommand(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _timeZoneBusinessLayer = timeZoneBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    [SlashCommand("time", "Get the current time for the specified user in their time zone.")]

    public async Task TimeSlashCommand(
        [Summary("User", "The user whose current time will be shown")]
        IUser user)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("That user is not a valid member of this server.");
        }

        await DeferAsync();

        try
        {
            var time = await _timeZoneBusinessLayer.GetTimeForPerson(user.Id);
            if (time == null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            var message = BuildTimeMessage(time.Value);
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed($"Current Time for {user.Username}",
                message, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            _logger.LogError(ex, "PersonNotFound in TimeSlashCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            _logger.LogError(ex, "NoTimeZone in TimeSlashCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in TimeSlashCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }

    private static string BuildTimeMessage(ZonedDateTime time)
    {
        var emoji = $"{TimeHelpers.GetEmojiForTime(time)}";
        return $"{emoji} **{TimeHelpers.FormatTime(time)}** on **{TimeHelpers.FormatDay(time)}**";
    }

    [MessageCommand("Get User's Current Time")]
    public async Task TimeMessageCommand(SocketMessage message)
    {
        var user = message.Author;

        await DeferAsync();

        try
        {
            var time = await _timeZoneBusinessLayer.GetTimeForPerson(user.Id);
            if (time == null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            var messageToSend = BuildTimeMessage(time.Value);
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed($"Current Time for {user.Username}",
                messageToSend, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            _logger.LogError(ex, "PersonNotFound in TimeMessageCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            _logger.LogError(ex, "NoTimeZone in TimeMessageCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in TimeMessageCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }

    [UserCommand("Get User's Current Time")]
    public async Task TimeUserCommand(SocketUser user)
    {
        await DeferAsync();

        try
        {
            var time = await _timeZoneBusinessLayer.GetTimeForPerson(user.Id);
            if (time == null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            var messageToSend = BuildTimeMessage(time.Value);
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed($"Current Time for {user.Username}",
                messageToSend, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            _logger.LogError(ex, "PersonNotFound in TimeUserCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            _logger.LogError(ex, "NoTimeZone in TimeUserCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in TimeUserCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }
}