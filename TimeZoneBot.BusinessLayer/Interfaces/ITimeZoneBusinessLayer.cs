using Discord;
using NodaTime;

namespace TimeZoneBot.BusinessLayer.Interfaces;

public interface ITimeZoneBusinessLayer
{
    Task<ZonedDateTime?> GetTimeForPerson(string userId);
    ZonedDateTime GetTimeInTimeZone(string timeZoneName);
    Task<ZonedDateTime> GetSpecificTimeForPerson(string targetUserId, string requesterUserId, string time);
    Task<bool> SetTimeZone(IUser user, string timeZone);
}