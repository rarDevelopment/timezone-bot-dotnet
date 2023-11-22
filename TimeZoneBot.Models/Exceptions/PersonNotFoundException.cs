namespace TimeZoneBot.Models.Exceptions;

public class PersonNotFoundException(string userId) : Exception($"No Person found with User ID: {userId}");