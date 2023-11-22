namespace TimeZoneBot.Models.Exceptions;

public class MissingMeridiemException(string timeInput) : Exception($"You must specify AM or PM - {timeInput}");