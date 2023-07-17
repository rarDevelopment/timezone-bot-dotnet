using NodaTime;
using NodaTime.Text;
using System.Text.RegularExpressions;
using Discord;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.DataLayer;
using TimeZoneBot.Models;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.BusinessLayer;

public class TimeZoneBusinessLayer : ITimeZoneBusinessLayer
{
    private readonly IPersonDataLayer _personDataLayer;
    private readonly IClock _clock;

    public TimeZoneBusinessLayer(IPersonDataLayer personDataLayer, IClock clock)
    {
        _personDataLayer = personDataLayer;
        _clock = clock;
    }

    public async Task<ZonedDateTime?> GetTimeForPerson(string userId)
    {
        var person = await GetPerson(userId);
        var timeZone = person!.TimeZone != null ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(person.TimeZone) : null;
        if (timeZone == null)
        {
            throw new NoTimeZoneException(userId);
        }

        var timeForPerson = GetTimeInTimeZone(timeZone, _clock.GetCurrentInstant());
        return timeForPerson;
    }

    public ZonedDateTime GetTimeInTimeZone(string timeZoneName)
    {
        var timeZone = DateTimeZoneProviders.Tzdb.GetZoneOrNull(timeZoneName) ?? null;
        if (timeZone == null)
        {
            throw new NoTimeZoneException(timeZoneName);
        }
        var timeInTimeZone = GetTimeInTimeZone(timeZone, _clock.GetCurrentInstant());
        return timeInTimeZone;
    }

    private async Task<Person?> GetPerson(string userId)
    {
        var person = await _personDataLayer.GetPerson(userId);
        return person;
    }

    public async Task<ZonedDateTime> GetSpecificTimeForPerson(string targetUserId, string requesterUserId, string time)
    {
        var targetPerson = await _personDataLayer.GetPerson(targetUserId);
        if (targetPerson == null)
        {
            throw new PersonNotFoundException(targetUserId);
        }

        var requestingPerson = await _personDataLayer.GetPerson(requesterUserId);
        if (requestingPerson == null)
        {
            throw new PersonNotFoundException(requesterUserId);
        }

        var targetTimeZone = targetPerson.TimeZone != null ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(targetPerson.TimeZone) : null;
        if (targetTimeZone == null)
        {
            throw new NoTimeZoneException(targetUserId);
        }

        var requesterTimeZone = requestingPerson.TimeZone != null
            ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(requestingPerson.TimeZone)
            : null;
        if (requesterTimeZone == null)
        {
            throw new NoTimeZoneException(requesterUserId);
        }

        var cleanedTime = CleanTime(time);

        LocalTime localTime;
        try
        {
            localTime = LocalTimePattern.CreateWithInvariantCulture(TimeHelpers.TimeFormat).Parse(cleanedTime).GetValueOrThrow();
        }
        catch (Exception)
        {
            localTime = LocalTimePattern.CreateWithInvariantCulture(TimeHelpers.TimeFormat24Hour).Parse(cleanedTime).GetValueOrThrow();
        }

        var requesterCurrentDate = _clock.GetCurrentInstant().InZone(requesterTimeZone).Date;

        var requesterTime = requesterCurrentDate + localTime;
        var requesterTimeZoned = requesterTime.InZoneLeniently(requesterTimeZone);

        var targetTime = requesterTimeZoned.ToInstant().InZone(targetTimeZone);
        return targetTime;
    }

    public async Task<bool> SetTimeZone(IUser user, string timeZone)
    {
        var userId = user.Id.ToString();
        var person = await _personDataLayer.GetPerson(userId);
        if (person == null)
        {
            throw new PersonNotFoundException(userId);
        }

        return await _personDataLayer.SetTimeZone(userId, timeZone);
    }

    private static string CleanTime(string time)
    {
        var multipleSpacesRegex = new Regex("[ ]{2,}");
        var cleanedTime = time.Replace(".", "").Trim();

        var allMeridiems = TimeHelpers.GetAllMeridiems();

        var meridiemFound = allMeridiems.FirstOrDefault(meridiem => TimeHelpers.HasMeridiem(meridiem, cleanedTime));

        var indexOfMeridiem = cleanedTime.IndexOf(meridiemFound.ToString(), StringComparison.InvariantCultureIgnoreCase);

        if (meridiemFound != Meridiem.NoMeridiem && indexOfMeridiem > 0) //accounts for -1 meaning none found and AM or PM not in the first position (which would not be valid time anyway)
        {
            if (cleanedTime[indexOfMeridiem - 1] != ' ')
            {
                cleanedTime = cleanedTime.Replace(meridiemFound.ToString(), $" {meridiemFound}", StringComparison.InvariantCultureIgnoreCase);
            }
        }

        cleanedTime = multipleSpacesRegex.Replace(cleanedTime, " ");

        return LocalTimePattern.CreateWithInvariantCulture(TimeHelpers.TimeFormatNoMinutes).Parse(cleanedTime).TryGetValue(new LocalTime(), out var shortTimeParsed)
            ? shortTimeParsed.ToString("h:mm tt", new System.Globalization.DateTimeFormatInfo())
            : cleanedTime;
    }

    private static ZonedDateTime GetTimeInTimeZone(DateTimeZone timeZone, Instant timeInstantToConvert)
    {
        return timeInstantToConvert.InZone(timeZone);
    }
}