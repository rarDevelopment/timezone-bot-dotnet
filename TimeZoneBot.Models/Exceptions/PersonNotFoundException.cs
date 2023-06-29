namespace TimeZoneBot.Models.Exceptions;

public class PersonNotFoundException : Exception
{
    public PersonNotFoundException(string userId) : base($"No Person found with User ID: {userId}") { }
}