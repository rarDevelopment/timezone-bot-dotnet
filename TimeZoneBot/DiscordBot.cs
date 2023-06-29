using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sgbj.Cron;
using TimeZoneBot.EventHandlers;
using TimeZoneBot.Models;
using TimeZoneBot.Notifications;

namespace TimeZoneBot;

public class DiscordBot : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactions;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger _logger;
    private readonly InteractionHandler _interactionHandler;
    private readonly BirthdayCheckHandler _birthdayCheckHandler;
    private readonly DiscordSettings _discordSettings;
    private readonly CancellationToken _cancellationToken;

    public DiscordBot(DiscordSocketClient client,
        InteractionService interactions,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<DiscordBot> logger,
        InteractionHandler interactionHandler,
        BirthdayCheckHandler birthdayCheckHandler,
        DiscordSettings discordSettings)
    {
        _client = client;
        _interactions = interactions;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
        _interactionHandler = interactionHandler;
        _birthdayCheckHandler = birthdayCheckHandler;
        _discordSettings = discordSettings;
        _cancellationToken = new CancellationTokenSource().Token;
    }

    private IMediator Mediator
    {
        get
        {
            var scope = _serviceScopeFactory.CreateScope();
            return scope.ServiceProvider.GetRequiredService<IMediator>();
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Ready += ClientReady;

        _client.Log += LogAsync;
        _interactions.Log += LogAsync;

        await _interactionHandler.InitializeAsync();

        SetEvents();

        await _client.LoginAsync(TokenType.Bot, _discordSettings.BotToken);

        await _client.SetActivityAsync(new Game("Use / commands!", ActivityType.Watching));

        await _client.StartAsync();

        await SetUpSchedules(stoppingToken);
    }

    private async Task SetUpSchedules(CancellationToken stoppingToken)
    {
        // birthday timer setup
        using var timer = new CronTimer("*/1 * * * *", TimeZoneInfo.Local);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await _birthdayCheckHandler.HandleBirthdayCheck();
        }
    }

    private async Task ClientReady()
    {
        _logger.LogInformation($"Logged as {_client.CurrentUser}");

        await _interactions.RegisterCommandsGloballyAsync();
    }

    private void SetEvents()
    {
        _client.MessageReceived += msg => Publish(new MessageReceivedNotification(msg));
        _client.ReactionAdded += (cacheableMessage, cacheableChannel, reaction) =>
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