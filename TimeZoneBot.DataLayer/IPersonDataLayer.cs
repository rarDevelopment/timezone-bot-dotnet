using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer;

public interface IPersonDataLayer
{
    Task<Person?> GetPerson(string userId);
    Task<bool> SetTimeZone(string userId, string timeZone);
    Task<bool> SetBirthday(string userId, string birthday);
}