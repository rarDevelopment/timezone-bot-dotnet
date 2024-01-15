using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models;

namespace TimeZoneBot.Commands;

public class SetDefaultTimeZoneCommand(IConfigurationBusinessLayer configurationBusinessLayer,
        IDiscordFormatter discordFormatter,
        DiscordSettings discordSettings,
        ILogger<DiscordBot> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("set-default-time-zone", "Set the default time zone for birthday checks when members have not set their time zone.")]
    public async Task SetDefaultTimeZoneSlashCommand(
        [Summary("time-zone", "Time Zone ID (use /link to find)")] string timeZoneName
        )
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("That user is not a valid member of this server.");
            return;
        }

        if (!member.GuildPermissions.Administrator)
        {
            await RespondAsync("You do not have permission to change this setting.");
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

        var wasSet = await configurationBusinessLayer.SetDefaultTimeZone(Context.Guild, timeZoneName);

        if (!wasSet)
        {
            logger.LogError($"Failed to set DefaultTimeZone to {timeZoneName} - SetDefaultTimeZone returned false.");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Failed to Set Time Reactions Configuration",
                "There was an error changing that setting.", Context.User));
            return;
        }

        await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter("Default Time Zone Set Successfully",
            $"Server Default Time Zone is set to **{timeZoneName}**", Context.User));
    }
}