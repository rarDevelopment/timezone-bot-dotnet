using NodaTime;

namespace TimeZoneBot.BusinessLayer.Interfaces;

public interface IBirthdayBusinessLayer
{
    Task<LocalDate?> GetBirthdayForPerson(ulong userId);
}