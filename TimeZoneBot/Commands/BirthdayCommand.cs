using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class BirthdayCommand(IBirthdayBusinessLayer birthdayBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("birthday", "Get the birthday for the specified user.")]

    public async Task BirthdaySlashCommand(
        [Summary("User", "The user whose birthday will be shown")]
        IUser user)
    {
        var member = Context.Guild.Users.FirstOrDefault(u => u.Id == Context.User.Id);
        if (member == null)
        {
            await RespondAsync("That user is not a valid member of this server.");
            return;
        }

        await DeferAsync();

        try
        {
            var birthday = await birthdayBusinessLayer.GetBirthdayForPerson(user.Id.ToString());
            if (birthday == null)
            {
                await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Birthday",
                    "Could not find birthday for person.", Context.User));
                return;
            }

            var message = BuildBirthdayMessage(birthday.Value);
            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"{user.GetNameToDisplay()}'s Birthday",
                message, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            logger.LogError(ex, "PersonNotFound in BirthdaySlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoBirthdayException ex)
        {
            logger.LogError(ex, "NoBirthday in BirthdaySlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Birthday Not Found",
                "No birthday is configured for this person.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in BirthdaySlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }

    private static string BuildBirthdayMessage(LocalDate birthday)
    {
        return $"🎈 **{BirthdayHelpers.FormatBirthday(birthday, true)}**";
    }

    [MessageCommand("Get User's Birthday")]
    public async Task BirthdayMessageCommand(SocketMessage message)
    {
        var user = message.Author;

        await DeferAsync();

        try
        {
            var birthdayForPerson = await birthdayBusinessLayer.GetBirthdayForPerson(user.Id.ToString());
            if (birthdayForPerson == null)
            {
                await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Birthday",
                    "Could not find a birthday for that person.", Context.User));
                return;
            }

            var messageToSend = BuildBirthdayMessage(birthdayForPerson.Value);
            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"{user.GetNameToDisplay()}'s Birthday",
                messageToSend, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            logger.LogError(ex, "PersonNotFound in BirthdayMessageCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoBirthdayException ex)
        {
            logger.LogError(ex, "NoBirthday in BirthdayMessageCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Birthday Not Found",
                "No birthday is configured for this person.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in BirthdayMessageCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }

    [UserCommand("Get User's Birthday")]
    public async Task BirthdayUserCommand(SocketUser user)
    {
        await DeferAsync();

        try
        {
            var birthdayForPerson = await birthdayBusinessLayer.GetBirthdayForPerson(user.Id.ToString());
            if (birthdayForPerson == null)
            {
                await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Birthday",
                    "Could not find a birthday for that person.", Context.User));
                return;
            }

            var messageToSend = BuildBirthdayMessage(birthdayForPerson.Value);
            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"{user.GetNameToDisplay()} 's Birthday",
                messageToSend, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            logger.LogError(ex, "PersonNotFound in BirthdayUserCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoBirthdayException ex)
        {
            logger.LogError(ex, "NoBirthday in BirthdayUserCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Birthday Not Found",
                "No birthday is configured for this person.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in BirthdayUserCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }
}