using Discord;
using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.DataLayer;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.BusinessLayer;

public class BirthdayBusinessLayer(IPersonDataLayer personDataLayer) : IBirthdayBusinessLayer
{
    public async Task<LocalDate?> GetBirthdayForPerson(string userId)
    {
        var person = await personDataLayer.GetPerson(userId);
        if (person == null)
        {
            throw new PersonNotFoundException(userId);
        }
        return person.Birthday;
    }

    public async Task<bool> SetBirthday(IUser user, LocalDate birthday)
    {
        var userId = user.Id.ToString();
        var person = await personDataLayer.GetPerson(userId);
        if (person == null)
        {
            throw new PersonNotFoundException(userId);
        }

        var birthdayString = BirthdayHelpers.FormatSortableBirthday(birthday);

        return await personDataLayer.SetBirthday(userId, birthdayString);
    }
}