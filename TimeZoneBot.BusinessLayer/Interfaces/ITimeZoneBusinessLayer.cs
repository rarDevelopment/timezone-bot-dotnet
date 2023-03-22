using Discord;
using NodaTime;

namespace TimeZoneBot.BusinessLayer.Interfaces;

public interface ITimeZoneBusinessLayer
{
    Task<ZonedDateTime?> GetTimeForPerson(ulong userId);
    ZonedDateTime GetTimeInTimeZone(string timeZoneName);
    Task<ZonedDateTime> GetSpecificTimeForPerson(ulong targetUserId, ulong requesterUserId, string time);
    Task<bool> SetTimeZone(IUser user, string timeZone);
}