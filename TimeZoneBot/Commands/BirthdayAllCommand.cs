using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class BirthdayAllCommand : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IBirthdayBusinessLayer _birthdayBusinessLayer;
    private readonly IDiscordFormatter _discordFormatter;
    private readonly ILogger<DiscordBot> _logger;

    public BirthdayAllCommand(IBirthdayBusinessLayer birthdayBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    {
        _birthdayBusinessLayer = birthdayBusinessLayer;
        _discordFormatter = discordFormatter;
        _logger = logger;
    }

    [SlashCommand("birthday-all", "Get the birthday for the specified user in their time zone.")]

    public async Task BirthdayAllSlashCommand()
    {
        var members =
            Context.Guild.Users.Where(u => u.GetPermissions(Context.Channel as IGuildChannel).ViewChannel && !u.IsBot);

        await DeferAsync();

        Dictionary<IUser, LocalDate> userBirthdays = new();

        foreach (var user in members)
        {
            try
            {
                var birthday = await _birthdayBusinessLayer.GetBirthdayForPerson(user.Id);
                if (birthday == null)
                {
                    continue;
                }

                userBirthdays.Add(user, birthday.Value);
            }
            catch (PersonNotFoundException ex)
            {
                _logger.LogError(ex, "PersonNotFound in BirthdaySlashCommand");
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Person Not Found",
                    "That person wasn't found!", Context.User));
            }
            catch (NoBirthdayException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error in BirthdaySlashCommand");
                await FollowupAsync(embed: _discordFormatter.BuildErrorEmbed("Something Went Wrong",
                    "There was an unexpected error.", Context.User));
            }

            var messagesToSend = userBirthdays.Select(userBirthday => BuildBirthdayMessage(userBirthday.Value, userBirthday.Key));
            var message = string.Join("\n", messagesToSend);
            await FollowupAsync(embed: _discordFormatter.BuildRegularEmbed($"{user.Username}'s Birthday",
                message, Context.User));
        }
    }

    private static string BuildBirthdayMessage(LocalDate birthday, IUser user)
    {
        return $"🎈 **{user.Username}**'s birthday is on **{BirthdayHelpers.FormatBirthday(birthday, true)}**";
    }
}