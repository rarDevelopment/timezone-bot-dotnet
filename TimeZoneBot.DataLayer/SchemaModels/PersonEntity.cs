using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using NodaTime;
using NodaTime.Text;
using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer.SchemaModels;

[BsonIgnoreExtraElements]
public class PersonEntity
{
    public PersonEntity(ulong snowflake)
    {
        Snowflake = snowflake;
    }
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("snowflake")]
    public ulong Snowflake { get; set; }

    [BsonElement("timezone")]
    public string? TimeZone { get; set; }

    [BsonElement("birthday")]
    public string? Birthday { get; set; }

    public Person ToDomain()
    {
        LocalDate? birthday = Birthday != null ? LocalDatePattern.Iso.Parse(Birthday).GetValueOrThrow() : null;
        return new Person(Snowflake, TimeZone, birthday);
    }
}