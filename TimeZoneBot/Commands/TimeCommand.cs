﻿using System.Text.RegularExpressions;
using DiscordDotNetUtilities.Interfaces;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class TimeCommand(ITimeZoneBusinessLayer timeZoneBusinessLayer,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("time", "Get the current time for the specified user in their time zone.")]

    public async Task TimeSlashCommand(
        [Summary("User", "The user whose current time will be shown")] IUser user,
        [Summary("Time", "The specific time in your time zone that you'd like to know in the other person's time zone.")] string? specifiedTime = null)
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
            var targetUserId = user.Id.ToString();
            if (string.IsNullOrEmpty(specifiedTime))
            {
                var time = await timeZoneBusinessLayer.GetTimeForPerson(targetUserId);
                if (time == null)
                {
                    await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Time",
                        "Could not find time for person.", Context.User));
                    return;
                }

                var message = TimeHelpers.BuildTimeMessage(time.Value);
                await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"Current Time for {user.GetNameToDisplay()}",
                    message, Context.User));
            }
            else
            {
                var timeRegex = new Regex(TimeHelpers.TimeRegexPattern, RegexOptions.IgnoreCase);
                if (!timeRegex.IsMatch(specifiedTime))
                {
                    await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error with Provided Time",
                        "The time provided was not a valid time format.", user));
                }

                var requesterUserId = Context.User.Id.ToString();

                var time = await timeZoneBusinessLayer.GetSpecificTimeForPerson(targetUserId, requesterUserId,
                    specifiedTime);

                var message = TimeHelpers.BuildSpecificTimeMessage(time.TimeOfDay, specifiedTime, user);
                await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter(
                    $"Specific Time Request for {user.GetNameToDisplay()}",
                    message, Context.User));
            }
        }
        catch (MissingMeridiemException ex)
        {
            logger.LogError(ex, "MissingMeridiem in TimeSlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Missing Meridiem",
                "You must specify AM or PM.", Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            logger.LogError(ex, "PersonNotFound in TimeSlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            logger.LogError(ex, "NoTimeZone in TimeSlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Time Zone Not Found",
                "No time zone was configured for this user. Use /set-time-zone.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in TimeSlashCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }

    [MessageCommand("Get User's Time")]
    public async Task TimeMessageCommand(SocketMessage message)
    {
        var user = message.Author;

        await DeferAsync();

        try
        {
            var time = await timeZoneBusinessLayer.GetTimeForPerson(user.Id.ToString());
            if (time == null)
            {
                await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            var messageToSend = TimeHelpers.BuildTimeMessage(time.Value);
            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"Current Time for {user.GetNameToDisplay()}",
                messageToSend, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            logger.LogError(ex, "PersonNotFound in TimeMessageCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            logger.LogError(ex, "NoTimeZone in TimeMessageCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in TimeMessageCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }

    [UserCommand("Get User's Time")]
    public async Task TimeUserCommand(SocketUser user)
    {
        await DeferAsync();

        try
        {
            var time = await timeZoneBusinessLayer.GetTimeForPerson(user.Id.ToString());
            if (time == null)
            {
                await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Error Finding Time",
                    "Could not find time for person.", Context.User));
                return;
            }

            var messageToSend = TimeHelpers.BuildTimeMessage(time.Value);
            await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter($"Current Time for {user.GetNameToDisplay()}",
                messageToSend, Context.User));
        }
        catch (PersonNotFoundException ex)
        {
            logger.LogError(ex, "PersonNotFound in TimeUserCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Person Not Found",
                "That person wasn't found!", Context.User));
        }
        catch (NoTimeZoneException ex)
        {
            logger.LogError(ex, "NoTimeZone in TimeUserCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Time Zone Not Found",
                "The associated time zone was not valid.", Context.User));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in TimeUserCommand");
            await FollowupAsync(embed: discordFormatter.BuildErrorEmbedWithUserFooter("Something Went Wrong",
                "There was an unexpected error.", Context.User));
        }
    }
}