﻿using DiscordDotNetUtilities.Interfaces;
using TimeZoneBot.Models;

namespace TimeZoneBot.Commands
{
    public class VersionCommand : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly VersionSettings _versionSettings;
        private readonly IDiscordFormatter _discordFormatter;

        public VersionCommand(VersionSettings versionSettings, IDiscordFormatter discordFormatter)
        {
            _versionSettings = versionSettings;
            _discordFormatter = discordFormatter;
        }

        [SlashCommand("version", "Get the current version number of the bot.")]
        public async Task VersionSlashCommand()
        {
            await RespondAsync(embed: _discordFormatter.BuildRegularEmbed("Bot Version",
                $"TimeZoneBot is at version **{_versionSettings.VersionNumber}**",
                Context.User));
        }
    }
}
