using NodaTime;
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
    }

    public interface IBirthdayBusinessLayer
    {
        Task<LocalDate?> GetBirthdayForPerson(ulong userId);
    }
}
