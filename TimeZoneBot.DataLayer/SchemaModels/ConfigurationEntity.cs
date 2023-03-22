using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using TimeZoneBot.Models;

namespace TimeZoneBot.DataLayer.SchemaModels;

[BsonIgnoreExtraElements]
public class ConfigurationEntity
{
    public ConfigurationEntity() { }

    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("guildId")]
    public ulong GuildId { get; set; }

    [BsonElement("guildName")]
    public string GuildName { get; set; }

    [BsonElement("enableReactions")]
    public bool EnableReactions { get; set; }

    public string? DefaultTimeZone { get; set; }

    public Configuration ToDomain()
    {
        return new Configuration
        {
            GuildId = GuildId,
            GuildName = GuildName,
            EnableReactions = EnableReactions,
            DefaultTimeZone = DefaultTimeZone,
        };
    }
}