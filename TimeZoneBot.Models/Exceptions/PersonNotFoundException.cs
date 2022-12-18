namespace TimeZoneBot.Models.Exceptions;

public class PersonNotFoundException : Exception
{
    public PersonNotFoundException(ulong userId) : base($"No Person found with User ID: {userId}") { }
}