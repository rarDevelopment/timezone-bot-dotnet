﻿using NodaTime;

namespace TimeZoneBot.BusinessLayer.Interfaces;

public interface ITimeZoneBusinessLayer
{
    Task<ZonedDateTime?> GetTimeForPerson(ulong userId);
    Task<ZonedDateTime> GetSpecificTimeForPerson(ulong targetUserId, ulong requesterUserId, string time);
}