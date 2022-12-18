namespace TimeZoneBot.Models.Exceptions;

public class NoTimeZoneException : Exception
{
    public NoTimeZoneException(ulong userId) : base($"No TimeZone found for person with User ID: {userId}") { }
}