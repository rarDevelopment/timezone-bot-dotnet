using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class SetTimeZoneCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly ITimeZoneBusinessLayer _timeZoneBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    public SetTimeZoneCommand(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _timeZoneBusinessLayer = timeZoneBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    [SlashCommand("set-time-zone", "Set your time zone.")]
    public async Task SetTimeZoneSlashCommand(
        [Summary("timezone", "Time Zone name (visit https://nodatime.org/TimeZones and choose a Zone ID).")]
        string timeZoneName)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("That user is not a valid member of this server.");
        }

        await DeferAsync();

        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneName);
        if (timeZone == null)
        {
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Invalid Time Zone",
                "The provided time zone was not valid. Please visit https://nodatime.org/TimeZones and choose a time zone from the Zone ID column.",
                member));
            return;
        }

        var wasSet = await _timeZoneBusinessLayer.SetTimeZone(Context.User, timeZoneName);

        if (!wasSet)
        {
            _logger.LogError($"Failed to set time zone with name {timeZoneName} - SetTimeZone returned false.");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Failed to Set Time Zone",
                "There was an error setting the time zone.", Context.User));
            return;
        }

        try
        {
            var time = await _timeZoneBusinessLayer.GetTimeForPerson(Context.User.Id);
            if (time == null)
            {
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Time Zone Set Successfully",
                $"Time Zone was successfully set to {timeZoneName}, and the current time should be: {TimeHelpers.FormatTime(time.Value.TimeOfDay)}", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            _logger.LogError(ex, "NoTimeZone in SetTimeZoneCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error in SetTimeZoneCommand");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }
}