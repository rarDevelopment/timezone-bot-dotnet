using DiscordDotNetUtilities.Interfaces;
using TimeZoneBot.Models;

namespace TimeZoneBot.Commands;

public class LinkCommand(IDiscordFormatter discordFormatter, DiscordSettings discordSettings)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("link-to-timezone-site", "Get a link to the website where you can find and set your time zone.")]
    public async Task LinkSlashCommand()
    {
        await RespondAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter(
            "Click here to visit the site where you can find and set your time zone",
            "You will have to log in with your Discord account and then click Settings under TimeZoneBot.",
            Context.User,
            url: discordSettings.TimeZoneConfigWebsite!));
    }
}