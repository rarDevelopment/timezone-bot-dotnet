using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using NodaTime.Text;
using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer.SchemaModels;

[BsonIgnoreExtraElements]
public class PersonEntity
{
    public PersonEntity(string userId)
    {
        UserId = userId;
    }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("timezone")]
    public string? TimeZone { get; set; }

    [BsonElement("birthday")]
    public string? Birthday { get; set; }

    public Person ToDomain()
    {
        LocalDate? birthday = Birthday != null ? LocalDatePattern.Iso.Parse(Birthday).GetValueOrThrow() : null;
        return new Person(UserId, TimeZone, birthday);
    }
}