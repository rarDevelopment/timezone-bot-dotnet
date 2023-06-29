namespace TimeZoneBot.Models.Exceptions;

public class NoBirthdayException : Exception
{
    public NoBirthdayException(string userId) : base($"No birthday found for user with id: {userId}") { }
}