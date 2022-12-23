﻿global using Discord;
global using Discord.Interactions;
global using Discord.WebSocket;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Logging;
using DiscordDotNetUtilities;
using DiscordDotNetUtilities.Interfaces;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Reflection;
using NodaTime;
using TimeZoneBot;
using TimeZoneBot.BusinessLayer;
using TimeZoneBot.BusinessLayer.Interfaces;
using TimeZoneBot.DataLayer;
using TimeZoneBot.Models;

var builder = new HostBuilder();

builder.ConfigureAppConfiguration(options
    => options.AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetEntryAssembly(), true)
        .AddEnvironmentVariables())
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.AddEnvironmentVariables(prefix: "DOTNET_");
    });

var loggerConfig = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File($"logs/log-{DateTime.Now:dd.MM.yy_HH.mm}.log")
    .CreateLogger();

builder.ConfigureServices((host, services) =>
{
    services.AddLogging(options => options.AddSerilog(loggerConfig, dispose: true));
    services.AddSingleton(new DiscordSocketClient(
        new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds |
                             GatewayIntents.GuildMembers |
                             GatewayIntents.GuildMessages |
                             GatewayIntents.GuildMessageReactions |
                             GatewayIntents.MessageContent,
            FormatUsersInBidirectionalUnicode = false,
            AlwaysDownloadUsers = true,
            LogGatewayIntentWarnings = false
        }));

    var discordSettings = new DiscordSettings
    {
        BotToken = host.Configuration["Discord:BotToken"]
    };
    var databaseSettings = new DatabaseSettings
    {
        Cluster = host.Configuration["Database:Cluster"],
        User = host.Configuration["Database:User"],
        Password = host.Configuration["Database:Password"],
        Name = host.Configuration["Database:Name"],
    };

    services.AddSingleton(discordSettings);
    services.AddSingleton(databaseSettings);

    services.AddScoped<IDiscordFormatter, DiscordFormatter>();

    services.AddScoped<ITimeZoneBusinessLayer, TimeZoneBusinessLayer>();
    services.AddScoped<IBirthdayBusinessLayer, BirthdayBusinessLayer>();
    services.AddScoped<IConfigurationBusinessLayer, ConfigurationBusinessLayer>();

    services.AddScoped<IPersonDataLayer, PersonDataLayer>();
    services.AddScoped<IConfigurationDataLayer, ConfigurationDataLayer>();

    IClock clock = SystemClock.Instance;
    services.AddTransient(_ => clock);

    services.AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>()));

    services.AddSingleton<InteractionHandler>();

    services.AddMediatR(typeof(DiscordBot));

    services.AddHostedService<DiscordBot>();
});

var app = builder.Build();

await app.RunAsync();
