using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Client;

public sealed class GameContext(ILogger<GameContext> logger, NavigationManager navigationManager)
    : IAsyncDisposable, IGameEventClient
{
    private IGameEventHub? _hub;
    private HubConnection? _hubConnection;

    public string? PlayerName { get; set; }

    private GameAggregate? Game { get; set; }

    public async ValueTask DisposeAsync()
    {
        await Stop();
    }

    public Task ReceiveGameEvent(IGameEvent gameEvent)
    {
        logger.LogInformation("Received {eventType}: {e}", gameEvent.GetType(), gameEvent);
        HandleGameEvent(gameEvent);
        return Task.CompletedTask;
    }

    private event Action? OnStateChange;

    public async Task<GameAggregate> Start(string gameId, string playerName, Action? stateChangeHandler)
    {
        Game = new GameAggregate(gameId, playerName);
        PlayerName = playerName;
        OnStateChange += stateChangeHandler;

        logger.LogDebug("GameContext: Started gameId {gameId} for {_playerName}", gameId, PlayerName);

        await StartHubConnection();

        return Game;
    }

    public async Task Stop()
    {
        if (_hubConnection != null) await _hubConnection.StopAsync();

        logger.LogDebug("GameContext: Stopped");
    }

    private void HandleGameEvent(IGameEvent e)
    {
        if (e.GameId != Game!.State.GameId) return;

        Game?.Handle(e);
        PublishEvents();
        OnStateChange?.Invoke();
    }

    public void PublishEvents()
    {
        if (!Game!.EventsToPublish.Any()) return;

        // TODO Check hub status
        logger.LogDebug("Publishing {count} events", Game.EventsToPublish.Count);

        foreach (var gameEvent in Game!.EventsToPublish)
            _hub!.BroadcastGameEvent(gameEvent.GetType().ToString(), gameEvent);

        Game.EventsToPublish.Clear();
    }

    private async Task StartHubConnection()
    {
        var hubUriBuilder = new UriBuilder(navigationManager.ToAbsoluteUri("/events"))
        {
            Query = "gameId=" + Uri.EscapeDataString(Game!.State.GameId)
                              + "&playerName=" + Uri.EscapeDataString(PlayerName ?? "")
        };

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUriBuilder.Uri)
            .Build();

        _hub = _hubConnection.ServerProxy<IGameEventHub>(); // todo rename interface
        _ = _hubConnection.ClientRegistration<IGameEventClient>(this);

        await _hubConnection.StartAsync();
        logger.LogInformation("Hub connection State: {state}", _hubConnection.State);
        // TODO Reconnection handling
    }
}
