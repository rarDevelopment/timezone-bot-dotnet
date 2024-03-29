﻿using DiscordDotNetUtilities.Interfaces;
using NodaTime;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.Commands;

public class BirthdayAllCommand(IBirthdayBusinessLayer birthdayBusinessLayer,
        IClock clock,
        IDiscordFormatter discordFormatter,
        ILogger<DiscordBot> logger)
    : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("birthday-all", "Get the birthdays for all users.")]

    public async Task BirthdayAllSlashCommand(
        [Summary("sort-by", "The method by which to sort the birthday list (defaults to Alphabetical sort by username)")] BirthdaySortOrder sortBy = BirthdaySortOrder.Alphabetical
    )
    {
        var members = Context.Guild.Users.Where(u => u.GetPermissions(Context.Channel as IGuildChannel).ViewChannel && !u.IsBot);

        await DeferAsync();

        var message = await BuildAllBirthdaysMessage(members, sortBy);

        var buttonBuilder = new ComponentBuilder()
            .WithButton("Sort By Age", $"birthdaySort:{BirthdaySortOrder.SortByAge}", emote: new Emoji("🧓🏼"))
            .WithButton("Sort By Next Birthday", $"birthdaySort:{BirthdaySortOrder.SortByNextBirthday}",
                emote: new Emoji("⏭️"))
            .WithButton("Sort Alphabetically", $"birthdaySort:{BirthdaySortOrder.Alphabetical}",
                emote: new Emoji("🔤"));

        await FollowupAsync(embed: discordFormatter.BuildRegularEmbedWithUserFooter("Birthdays", message, Context.User), components: buttonBuilder.Build());
    }

    private async Task<string> BuildAllBirthdaysMessage(IEnumerable<SocketGuildUser> members,
        BirthdaySortOrder sortType)
    {
        Dictionary<IUser, LocalDate> userBirthdays = new();

        foreach (var user in members)
        {
            try
            {
                var birthday = await birthdayBusinessLayer.GetBirthdayForPerson(user.Id.ToString());
                if (birthday == null)
                {
                    continue;
                }

                userBirthdays.Add(user, birthday.Value);
            }
            catch (PersonNotFoundException) { }
            catch (NoBirthdayException) { }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled error in BirthdaySlashCommand");
            }
        }

        IOrderedEnumerable<KeyValuePair<IUser, LocalDate>> orderedBirthdays;
        switch (sortType)
        {
            case BirthdaySortOrder.SortByAge:
                orderedBirthdays = userBirthdays.OrderBy(b => b.Value);
                break;
            case BirthdaySortOrder.SortByNextBirthday:
                orderedBirthdays = userBirthdays.OrderBy(ub =>
                {
                    var annualDate = new AnnualDate(ub.Value.Month, ub.Value.Day);
                    // TODO: determine if we can depend on having the user's timezone here
                    var lenientBirthday = GetLenientBirthday(annualDate, clock.GetCurrentInstant().InUtc().Year);
                    if (lenientBirthday < clock.GetCurrentInstant().InUtc().Date)
                    {
                        lenientBirthday = new LocalDate(lenientBirthday.Year + 1, lenientBirthday.Month,
                            lenientBirthday.Day);
                    }
                    return lenientBirthday;
                });
                break;
            case BirthdaySortOrder.Alphabetical:
            default:
                orderedBirthdays = userBirthdays.OrderBy(u => u.Key.GetNameToDisplay());
                break;
        }

        var messagesToSend = orderedBirthdays.Select(userBirthday => BuildBirthdayMessage(userBirthday.Value, userBirthday.Key));
        var message = string.Join("\n", messagesToSend);
        return message;
    }

    private static LocalDate GetLenientBirthday(AnnualDate annualDate, int year)
    {
        if (annualDate is { Month: 2, Day: 29 })
        {
            return new LocalDate(year, 2, 28);
        }

        return new LocalDate(year, annualDate.Month, annualDate.Day);
    }

    [ComponentInteraction("birthdaySort:*")]
    public async Task SortButton(string sortingTypeParam)
    {
        await DeferAsync();
        var members = Context.Guild.Users.Where(u => u.GetPermissions(Context.Channel as IGuildChannel).ViewChannel && !u.IsBot);
        if (!Enum.TryParse(typeof(BirthdaySortOrder), sortingTypeParam, out var sortingTypeParsed))
        {
            logger.LogError("Error in SortButton, could not parse sort type.");
            return;
        }

        var birthdaySortOrderType = (BirthdaySortOrder)sortingTypeParsed;
        var message = await BuildAllBirthdaysMessage(members, birthdaySortOrderType);
        await Context.Interaction.ModifyOriginalResponseAsync(properties =>
        {
            properties.Embed =
                discordFormatter.BuildRegularEmbedWithUserFooter($"Birthdays ({ParseSortByName(birthdaySortOrderType)})",
                    message, Context.User);
        });
    }

    private static string ParseSortByName(BirthdaySortOrder birthdaySortOrder)
    {
        return birthdaySortOrder switch
        {
            BirthdaySortOrder.SortByAge => "Sorted by Age",
            BirthdaySortOrder.SortByNextBirthday => "Sorted by Next Birthday",
            BirthdaySortOrder.Alphabetical => "Sorted Alphabetically",
            _ => ""
        };
    }

    private static string BuildBirthdayMessage(LocalDate birthday, IUser user)
    {
        return $"🎈 **{user.GetNameToDisplay()}**'s birthday is on **{BirthdayHelpers.FormatBirthday(birthday, true)}**";
    }
}