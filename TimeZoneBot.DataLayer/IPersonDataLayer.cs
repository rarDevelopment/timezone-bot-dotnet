using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer;

public interface IPersonDataLayer
{
    Task<Person?> GetPerson(ulong userId);
    Task<bool> SetTimeZone(ulong userId, string timeZone);
    Task<bool> SetBirthday(ulong userId, string birthday);
}