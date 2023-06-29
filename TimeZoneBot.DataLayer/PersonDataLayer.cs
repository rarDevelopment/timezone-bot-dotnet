using MongoDB.Driver;
using TimeZoneBot.DataLayer.SchemaModels;
using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer;

public class PersonDataLayer : IPersonDataLayer
{
    private readonly IMongoCollection<PersonEntity> _personCollection;

    public PersonDataLayer(DatabaseSettings databaseSettings)
    {
        var connectionString = $"mongodb+srv://{databaseSettings.User}:{databaseSettings.Password}@{databaseSettings.Cluster}.mongodb.net/{databaseSettings.Name}?w=majority";
        var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseSettings.Name);
        _personCollection = database.GetCollection<PersonEntity>("person");
    }
    public async Task<Person?> GetPerson(string userId)
    {
        var filter = Builders<PersonEntity>.Filter.Eq(p => p.UserId, userId);
        var person = await _personCollection.Find(filter).FirstOrDefaultAsync();
        if (person != null)
        {
            return person.ToDomain();
        }
        await _personCollection.InsertOneAsync(new PersonEntity(userId));
        person = await _personCollection.Find(filter).FirstOrDefaultAsync();
        return person?.ToDomain();
    }

    public async Task<bool> SetTimeZone(string userId, string timeZone)
    {
        var person = await GetPerson(userId);
        if (person == null)
        {
            return false;
        }
        var filter = Builders<PersonEntity>.Filter.Eq(p => p.UserId, userId);
        var update = Builders<PersonEntity>.Update.Set(p => p.TimeZone, timeZone);
        var updateResult = await _personCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }

    public async Task<bool> SetBirthday(string userId, string birthday)
    {
        var person = await GetPerson(userId);
        if (person == null)
        {
            return false;
        }
        var filter = Builders<PersonEntity>.Filter.Eq(p => p.UserId, userId);
        var update = Builders<PersonEntity>.Update.Set(p => p.Birthday, birthday);
        var updateResult = await _personCollection.UpdateOneAsync(filter, update);
        return updateResult.MatchedCount == 1;
    }
}