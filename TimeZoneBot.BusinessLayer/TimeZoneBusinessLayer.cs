using NodaTime;
using TimeZoneBot.DataLayer;
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

    public async Task<ZonedDateTime?> GetTimeForPerson(ulong userId)
    {
        var person = await _personDataLayer.GetPerson(userId);
        if (person == null)
        {
            throw new PersonNotFoundException(userId);
        }
        var timeZone = person.TimeZone != null ? DateTimeZoneProviders.Tzdb.GetZoneOrNull(person.TimeZone) : null;
        if (timeZone == null)
        {
            throw new NoTimeZoneException(userId);
        }

        var timeForPerson = GetTimeInTimeZone(timeZone, _clock.GetCurrentInstant());
        return timeForPerson;
    }

    public async Task<LocalTime> GetSpecificTimeForPerson(ulong targetUserId, ulong requesterUserId, string time)
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

        var timeSplit = time.Trim().Split(":");
        var hours = Convert.ToInt32(timeSplit[0]);
        var minutes = timeSplit.Length >= 2 ? Convert.ToInt32(timeSplit[1]) : 0;
        var seconds = timeSplit.Length >= 3 ? Convert.ToInt32(timeSplit[2]) : 0;

        var requesterCurrentDate = _clock.GetCurrentInstant().InZone(requesterTimeZone).Date;
        var localTime = new LocalTime(hours, minutes, seconds);

        var requesterTime = requesterCurrentDate + localTime;
        var requesterTimeZoned = requesterTime.InZoneLeniently(requesterTimeZone);

        var targetTime = requesterTimeZoned.ToInstant().InZone(targetTimeZone);
        
        return targetTime.TimeOfDay;
    }

    private static ZonedDateTime GetTimeInTimeZone(DateTimeZone timeZone, Instant timeInstantToConvert)
    {
        return timeInstantToConvert.InZone(timeZone);
    }
}

public interface ITimeZoneBusinessLayer
{
    Task<ZonedDateTime?> GetTimeForPerson(ulong userId);
    Task<LocalTime> GetSpecificTimeForPerson(ulong targetUserId, ulong requesterUserId, string time);
}