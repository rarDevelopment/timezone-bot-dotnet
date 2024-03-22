# TimeZoneBot

TimeZoneBot has 2 main features:

- Converting times between time zones for users in your Discord
- Checking users birthdays (and a birthday post on their birthday)

Now written in C# and .NET 8.

## Links

[Invite TimeZoneBot to your Discord Server](https://discord.com/api/oauth2/authorize?client_id=736720417166721105&permissions=414464859200&scope=bot%20applications.commands)

[Discord Support Server](https://discord.gg/Za4NAtJJ9v)

![TimeZoneBot](https://github.com/rarDevelopment/timezone-bot-dotnet/assets/4060573/52f12fc5-6d37-4e6f-91a1-8493097d71cd)

## Getting Started

- Invite the bot to your server using the invite link above.
- Each user will have to set up their time zone. To do this, it is recommended that you visit [the TimeZoneBot website](https://rardk.com/bots/timezonebot) using the instructions below.
- Each user can also set up their birthday. For birthday alerts to work, the user will have to set up their time zone as well.
- **NOTE:** If your server is made up of users who are entirely in the same time zone, and you just wish to use this bot for birthday wishes, you can set a default time zone using the `/set-default-time-zone` command - for more on this command, see the [Commands section](#commands) below.

## Using the Website

- Browse to [the TimeZoneBot website](https://rardk.com/bots/timezonebot/). If you're brought to the home page with a list of bots, click Settings under TimeZoneBot.
- You'll be asked to log into your Discord account and authorize for the website. Click Authorize.
- You'll be redirected to the time zone list.
- Search for your time zone, and either click the "Set" button to set it immediately, or use the copy icon button to copy it and use it with either the `/set-time-zone` or `/set-default-time-zone` commands in Discord.

![image](https://github.com/rarDevelopment/timezone-bot-dotnet/assets/4060573/03e1ba34-ab22-49bb-8f9c-950f95c4e19f)

## Time Reactions

When TimeZoneBot detects a valid time in a user's message, it will react with a "Time" emote reaction. If you tap that reaction, TimeZoneBot will respond with that time converted to your time. You must have your time zone set up for this to work.

![Time Reaction](https://github.com/rarDevelopment/timezone-bot-dotnet/assets/4060573/cb8686ce-49ca-430c-b99c-7be4d0a342a3)

## Commands

**Note:** Commands or descriptions below with * can only be used by the administrators.

### Time Commands

---

`/time`

Get the current time for the specified user in their time zone. Optionally, can also specify a time, and it will convert that time to the specified user's time zone.

--- 

`/time-all`

Get the current time for the all users in their respective time zones. Optionally, you can also specify a time, and it will convert that time to all other user's time zones.

---

`/set-time-zone`

Sets your time zone. NOTE: You must use a valid IANA Zone ID. You can [use our website](#using-the-website) to set it more easily.

---

`/set-default-time-zone`*

Set the default time zone for birthday checks when members have not set their time zone. NOTE: You must use a valid IANA Zone ID. You can [use our website](#using-the-website) to find a valid ID.

---

`/set-reactions`*

Set [time reactions](#time-reactions) on/off.

---

### Birthday Commands

---

`/birthday`

Get the birthday for the specified user.

---

`/birthday-all`

Get the birthdays for all users. You can also specify a method by which to sort the birthdays: Alphabetical (by username), Age, or Next Birthday.

---

`/set-birthday`

Set your birthday.

---

`/set-birthday-announcements`*

Set birthday announcements for the server on/off.

---
