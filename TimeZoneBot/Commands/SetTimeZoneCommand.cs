using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class SetTimeZoneCommand(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IDiscordFormatter discordFormatter,
        DiscordSettings discordSettings,
        ILogger<DiscordBot> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("set-time-zone", "Set your time zone (recommended: use /link to set this instead).")]
    public async Task SetTimeZoneSlashCommand(
        [Summary("time-zone", "Time Zone ID")]
        string timeZoneName)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("That user is not a valid member of this server.");
            return;
        }

        await DeferAsync();

        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneName);
        if (timeZone == null)
        {
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Invalid Time Zone",
                $"The provided time zone was not valid. Please visit {discordSettings.TimeZoneConfigWebsite} and set your time zone there, or copy it and set it here.",
                member));
            return;
        }

        var wasSet = await timeZoneBusinessLayer.SetTimeZone(Context.User, timeZoneName);

        if (!wasSet)
        {
            logger.LogError($"Failed to set time zone with name {timeZoneName} - SetTimeZone returned false.");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Failed to Set Time Zone",
                "There was an error setting the time zone.", Context.User));
            return;
        }

        try
        {
            var time = await timeZoneBusinessLayer.GetTimeForPerson(Context.User.Id.ToString());
            if (time == null)
            {
                await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter("Time Zone Set Successfully",
                $"Time Zone was successfully set to {timeZoneName}, and the current time should be: {TimeHelpers.FormatTime(time.Value.TimeOfDay)}", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            logger.LogError(ex, "NoTimeZone in SetTimeZoneCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in SetTimeZoneCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }
}