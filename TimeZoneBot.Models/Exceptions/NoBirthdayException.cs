namespace TimeZoneBot.Models.Exceptions;

public class NoBirthdayException(string userId) : Exception($"No birthday found for user with id: {userId}");