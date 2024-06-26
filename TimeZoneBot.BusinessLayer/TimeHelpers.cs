﻿using System.Globalization;
using Discord;
using NodaTime;

namespace TimeZoneBot.BusinessLayer;

public static class TimeHelpers
{
    public const string TimeRegexPattern =
        @"\b(([0-1]?[0-9]|2[0-3]):[0-5][0-9]( |)([ap]\.?m\.?)?|([0-1]?[0-9]|2[0-3])( |)([ap]\.?m\.?))\b";
    public const string TimeFormat = "h:mm tt";
    public const string TimeFormatNoMinutes = "h tt";
    public const string TimeFormat24Hour = "H:mm";
    public const string DayFormat = "dddd, dd MMMM";
    public const ulong TimeButtonEmojiId = 819015682871001108;
    public const string TimeButtonEmojiName = "time_button";

    public static bool HasMeridiem(Meridiem meridiem, string time)
    {
        return time.Replace(".", "").ToLower().Contains(meridiem.ToString().ToLower());
    }

    public static string TimeWithMeridiem(string time)
    {
        if (HasAnyMeridiem(time))
        {
            return time;
        }

        var splitTime = time.Split(':');
        if (splitTime.Length == 0)
        {
            return time;
        }

        var hourString = splitTime[0];
        var minuteString = splitTime[1];

        if (splitTime.Length <= 0 || !int.TryParse(hourString, out var hour))
        {
            return time;
        }

        var meridiemToUse = Meridiem.AM;
        if (hour >= 12)
        {
            meridiemToUse = Meridiem.PM;
            if (hour > 12)
            {
                hour -= 12;
            }
        }
        if (hour == 0)
        {
            hour = 12;
        }

        return $"{hour}{(!string.IsNullOrEmpty(minuteString) ? $":{minuteString}" : "")} {meridiemToUse}";

    }

    private static bool HasAnyMeridiem(string time)
    {
        var allValues = GetAllMeridiems();
        return allValues.Any(meridiem => HasMeridiem(meridiem, time));
    }

    public static Meridiem[] GetAllMeridiems()
    {
        return (Meridiem[])Enum.GetValues(typeof(Meridiem));
    }

    public static string FormatTime(LocalTime time)
    {
        return time.ToString(TimeFormat, new DateTimeFormatInfo());
    }

    public static string FormatDay(ZonedDateTime time)
    {
        return time.ToString(DayFormat, new DateTimeFormatInfo());
    }

    public static string GetEmojiForTime(LocalTime time)
    {
        var formattedTime = FormatTime(time);
        const string replaceText = "{{REPLACE}}";
        var emoji = $":clock{replaceText}:";
        var timeSplit = formattedTime.Split(" ")[0].Split(":"); //first split on space is to remove a.m. or p.m.
        var minutes = Convert.ToInt32(timeSplit[1]);
        emoji = emoji.Replace(replaceText, minutes >= 30
            ? $"{timeSplit[0]}30"
            : $"{timeSplit[0]}");
        return emoji;
    }

    public static Emote GetTimeButtonEmote()
    {
        return new Emote(TimeButtonEmojiId, TimeButtonEmojiName);
    }

    public static string BuildSpecificTimeMessage(LocalTime time, string specifiedTime, IUser user)
    {
        var timeWithMeridiem = TimeWithMeridiem(specifiedTime);
        return $"At **{timeWithMeridiem}** your time, it will be **{FormatTime(time)}** in {user.GetNameToDisplay()}'s time.";
    }

    public static string BuildSpecificTimeReactionMessage(LocalTime time, string specifiedTime, IUser user)
    {
        var timeWithMeridiem = TimeWithMeridiem(specifiedTime);
        return $"At **{timeWithMeridiem}** in {user.GetNameToDisplay()}'s time, it will be **{FormatTime(time)}** in your time.";
    }

    public static string BuildTimeMessage(ZonedDateTime time)
    {
        var emoji = $"{GetEmojiForTime(time.TimeOfDay)}";
        return $"{emoji} **{FormatTime(time.TimeOfDay)}** on **{FormatDay(time)}**";
    }
}