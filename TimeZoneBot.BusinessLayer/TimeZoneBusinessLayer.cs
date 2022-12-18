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

    private static ZonedDateTime GetTimeInTimeZone(DateTimeZone timeZone, Instant timeInstantToConvert)
    {
        return timeInstantToConvert.InZone(timeZone);
    }
}

public interface ITimeZoneBusinessLayer
{
    Task<ZonedDateTime?> GetTimeForPerson(ulong userId);
}