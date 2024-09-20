using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Client;

public sealed class GameContext(ILogger<GameContext> logger, NavigationManager navigationManager)
    : IGameEventClient, IGameContext
{
    private IGameEventHub? _hub;
    private HubConnection? _hubConnection;

    public GameAggregate? Game { get; set; }

    public string? PlayerName { get; set; }

    public async Task Initialize(Action? stateChangeHandler)
    {
        OnStateChange += stateChangeHandler;
        await StartHubConnection();
    }

    public GameAggregate CreateGame(string gameId, string playerName)
    {
        Game = new GameAggregate(gameId, playerName);
        PlayerName = playerName;

        return Game;
    }

    public void PublishEvents()
    {
        if (!Game!.EventsToPublish.Any()) return;

        logger.LogDebug("Publishing {count} events", Game.EventsToPublish.Count);

        foreach (var gameEvent in Game!.EventsToPublish)
            _hub!.BroadcastGameEvent(gameEvent);

        Game.EventsToPublish.Clear();
    }

    public ConnectionStatus Status => _hubConnection?.State switch
    {
        HubConnectionState.Connected => ConnectionStatus.Connected,
        HubConnectionState.Connecting => ConnectionStatus.Connecting,
        HubConnectionState.Reconnecting => ConnectionStatus.Connecting,
        HubConnectionState.Disconnected => ConnectionStatus.Failed,
        _ => ConnectionStatus.Failed
    };

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null) await _hubConnection.DisposeAsync();
    }

    public Task ReceiveGameEvent(IGameEvent gameEvent)
    {
        logger.LogDebug("Received {eventType}: {e}", gameEvent.GetType(), gameEvent);
        HandleGameEvent(gameEvent);
        return Task.CompletedTask;
    }

    private event Action? OnStateChange;

    private void HandleGameEvent(IGameEvent e)
    {
        if (e.GameId != Game!.State.GameId) return;

        Game?.Handle(e);
        PublishEvents();
        OnStateChange?.Invoke();
    }

    private async Task StartHubConnection()
    {
        var hubUriBuilder = new UriBuilder(navigationManager.ToAbsoluteUri("/events"));

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUriBuilder.Uri)
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.Closed += OnHubConnectionClosed;
        _hubConnection.Reconnecting += OnHubConnectionReconnecting;
        _hubConnection.Reconnected += OnHubConnectionReconnected;

        _hub = _hubConnection.ServerProxy<IGameEventHub>();
        _hubConnection.ClientRegistration<IGameEventClient>(this);

        await _hubConnection.StartAsync();
        logger.LogInformation("Hub connection State: {state} id: {id}", _hubConnection.State,
            _hubConnection.ConnectionId);
    }

    private Task OnHubConnectionReconnected(string? s)
    {
        if (Game is not null)
        {
            // On rejoin, clear user vote and game status then publish join
            // event so other players sync up
            Game.Handle(new GameReset(Game.State.GameId));
            Game.PlayerJoined();
            PublishEvents();
        }

        OnStateChange?.Invoke();
        logger.LogInformation("Hub reconnected: {s}", s);
        return Task.CompletedTask;
    }

    private Task OnHubConnectionReconnecting(Exception? s)
    {
        logger.LogInformation("Hub reconnecting: {s}", s);
        OnStateChange?.Invoke();
        return Task.CompletedTask;
    }

    private Task OnHubConnectionClosed(Exception? s)
    {
        logger.LogInformation("Hub connection closed: {s}", s);
        OnStateChange?.Invoke();
        return Task.CompletedTask;
    }
}
