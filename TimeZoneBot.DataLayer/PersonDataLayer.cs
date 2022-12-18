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
    public async Task<Person?> GetPerson(ulong userId)
    {
        var filter = Builders<PersonEntity>.Filter.Eq("snowflake", userId.ToString());
        var person = await _personCollection.Find(filter).FirstOrDefaultAsync();
        return person?.ToDomain();
    }
}

public interface IPersonDataLayer
{
    Task<Person?> GetPerson(ulong userId);
}