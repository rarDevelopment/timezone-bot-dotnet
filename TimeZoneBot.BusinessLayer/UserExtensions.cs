using Discord;

namespace TimeZoneBot.BusinessLayer;

public static class UserExtensions
{
    public static string GetNameToDisplay(this IUser user)
    {
        return user.GlobalName ?? user.Username;
    }
}