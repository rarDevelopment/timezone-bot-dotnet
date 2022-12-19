namespace TimeZoneBot.Models.Exceptions
{
    public class MissingMeridiemException : Exception
    {
        public MissingMeridiemException(string timeInput) : base($"You must specify AM or PM - {timeInput}") { }
    }
}
