using DiscordDotNetUtilities.Interfaces;
using TimeZoneBot.BusinessLayer.Interfaces;

namespace TimeZoneBot.Commands;

public class ViewSettingsCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;

    public ViewSettingsCommand(IConfigurationBusinessLayer configurationBusinessLayer, IDiscordFormatter discordFormatter)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _discordFormatter = discordFormatter;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("view-settings", "See the current settings for the bot.")]
    public async Task ViewSettings()
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("Hmm, something is wrong, you aren't able to do that.");
            return;
        }
        if (member.GuildPermissions.Administrator)
        {
            var guildConfig = await _configurationBusinessLayer.GetConfiguration(Context.Guild);

            var message = "";
            message += $"Birthday Announcements: {GetEnabledText(guildConfig.EnableBirthdayAnnouncements)}\n";
            message += $"Time Reactions: {GetEnabledText(guildConfig.EnableReactions)}\n";
            message += $"Default Time Zone: {(!string.IsNullOrEmpty(guildConfig.DefaultTimeZone) ? guildConfig.DefaultTimeZone : "Not Set")}\n";

            await RespondAsync(embed: _discordFormatter.BuildRegularEmbed($"Settings for {Context.Guild.Name}",
                message,
                Context.User));
            return;
        }

        await RespondAsync("You aren't allowed to do that!");
    }

    private static string GetAdminUserDisplayText(IEnumerable<string> adminUserIds)
    {
        return string.Join(", ", adminUserIds.Select(s => $"<@{s}>"));
    }

    private static string GetEnabledText(bool isEnabled)
    {
        return isEnabled ? "ON" : "OFF";
    }
}