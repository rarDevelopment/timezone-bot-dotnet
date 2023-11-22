using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sgbj.Cron;
using TimeZoneBot.EventHandlers;
using TimeZoneBot.Models;
using TimeZoneBot.Notifications;

namespace TimeZoneBot;

public class DiscordBot(DiscordSocketClient client,
        InteractionService interactions,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DiscordBot> logger,
        InteractionHandler interactionHandler,
        BirthdayCheckHandler birthdayCheckHandler,
        DiscordSettings discordSettings)
    : BackgroundService
{
    private readonly ILogger _logger = logger;
    private readonly CancellationToken _cancellationToken = new CancellationTokenSource().Token;

    private IMediator Mediator
    {
        get
        {
            var scope = serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        client.Ready += ClientReady;

        client.Log += LogAsync;
        interactions.Log += LogAsync;

        await interactionHandler.InitializeAsync();

        SetEvents();

        await client.LoginAsync(TokenType.Bot, discordSettings.BotToken);

        await client.SetActivityAsync(new Game("Use / commands!", ActivityType.Watching));

        await client.StartAsync();

        await SetUpSchedules(stoppingToken);
    }

    private async Task SetUpSchedules(CancellationToken stoppingToken)
    {
        // birthday timer setup
        using var timer = new CronTimer("1 * * * *", TimeZoneInfo.Local);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await birthdayCheckHandler.HandleBirthdayCheck();
        }
    }

    private async Task ClientReady()
    {
        _logger.LogInformation($"Logged as {client.CurrentUser}");

        await interactions.RegisterCommandsGloballyAsync();
    }

    private void SetEvents()
    {
        client.MessageReceived += msg => Publish(new MessageReceivedNotification(msg));
        client.ReactionAdded += (cacheableMessage, cacheableChannel, reaction) =>
            Publish(new ReactionAddedNotification(cacheableMessage, cacheableChannel, reaction));
    }

    private Task Publish<TEvent>(TEvent @event) where TEvent : INotification
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Mediator.Publish(@event, _cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception in {Event}:  {ExceptionMessage}", @event.GetType().Name, ex.Message);
            }
        }, _cancellationToken);
        return Task.CompletedTask;
    }

    public async Task LogAsync(LogMessage msg)
    {
        var severity = msg.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Trace,
            LogSeverity.Debug => LogLevel.Debug,
            _ => LogLevel.Information
        };

        _logger.Log(severity, msg.Exception, msg.Message);

        await Task.CompletedTask;
    }
}