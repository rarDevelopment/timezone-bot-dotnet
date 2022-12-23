using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Commands.Enums;

namespace TimeZoneBot.Commands;

public class SetBirthdayCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBirthdayBusinessLayer _birthdayBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    public SetBirthdayCommand(IBirthdayBusinessLayer birthdayBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _birthdayBusinessLayer = birthdayBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    [SlashCommand("set-birthday", "Set your birthday.")]
    public async Task SetBirthdaySlashCommand(
        [Summary("month", "The month of your birthday.")] BirthdayMonths month,
        [Summary("day", "The day of the month of your birthday.")][MaxValue(31)][MinValue(1)] int day,
        [Summary("year", "The year of your birthday.")] int year
        )
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("That user is not a valid member of this server.");
            return;
        }

        await DeferAsync();

        LocalDate birthdayDate;

        try
        {
            birthdayDate = new LocalDate(year, (int)month, day);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse birthday from entered date");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Invalid Birthday",
                "The provided birthday was not valid.",
                member));
            return;
        }

        var wasSet = await _birthdayBusinessLayer.SetBirthday(Context.User, birthdayDate);

        if (!wasSet)
        {
            _logger.LogError($"Failed to set birthday to {birthdayDate} - SetBirthday returned false.");
            await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Failed to Set Time Zone",
                "There was an error setting the time zone.", Context.User));
            return;
        }

        await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed("Birthday Set Successfully",
            $"Birthday was successfully set to {birthdayDate}", Context.User));
    }
}