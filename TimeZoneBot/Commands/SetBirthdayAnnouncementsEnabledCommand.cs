﻿using DiscordDotNetUtilities.Interfaces;
using TimeZoneBot.BusinessLayer.Interfaces;

namespace TimeZoneBot.Commands;

public class SetBirthdayAnnouncementsEnabledCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IConfigurationBusinessLayer _configurationBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    public SetBirthdayAnnouncementsEnabledCommand(IConfigurationBusinessLayer configurationBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _configurationBusinessLayer = configurationBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    [DefaultMemberPermissions(GuildPermission.Administrator)]
    [SlashCommand("set-birthday-announcements", "Set birthday announcements on/off.")]
    public async Task SetReactionsEnabledSlashCommand(
        [Summary("enabled", "True for ON, False for OFF")] bool isEnabled
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

        var wasSet = await _configurationBusinessLayer.SetBirthdayAnnouncementsEnabled(Context.Guild, isEnabled);

        if (!wasSet)
        {
            _logger.LogError($"Failed to set EnableBirthdayAnnouncements to {isEnabled} - SetReactionsEnabled returned false.");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Failed to Set Birthday Announcements Configuration",
                "There was an error changing that setting.", Context.User));
            return;
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Birthday Announcements Configuration Set Successfully",
            $"Birthday Announcements are **{(isEnabled ? "ON" : "OFF")}**", Context.User));
    }
}