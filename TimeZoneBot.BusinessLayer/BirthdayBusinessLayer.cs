using Discord;
using NodaTime;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.DataLayer;
using TimeZoneBot.Models.Exceptions;

namespace TimeZoneBot.BusinessLayer
{
    public class BirthdayBusinessLayer : IBirthdayBusinessLayer
    {
        private readonly IPersonDataLayer _personDataLayer;

        public BirthdayBusinessLayer(IPersonDataLayer personDataLayer)
        {
            _personDataLayer = personDataLayer;
        }

        public async Task<LocalDate?> GetBirthdayForPerson(ulong userId)
        {
            var person = await _personDataLayer.GetPerson(userId);
            if (person == null)
            {
                throw new PersonNotFoundException(userId);
            }
            return person.Birthday;
        }

        public async Task<bool> SetBirthday(IUser user, LocalDate birthday)
        {
            var person = await _personDataLayer.GetPerson(user.Id);
            if (person == null)
            {
                throw new PersonNotFoundException(user.Id);
            }

            var birthdayString = BirthdayHelpers.FormatSortableBirthday(birthday);

            return await _personDataLayer.SetBirthday(user.Id, birthdayString);
        }
    }
}
