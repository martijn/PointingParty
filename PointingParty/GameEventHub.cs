using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty;

public class GameEventHub : Hub<IGameEventClient>, IGameEventHub
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<GameEventHub> _logger;

    public GameEventHub(ILogger<GameEventHub> logger)
    {
        _logger = logger;
    }

    public Task BroadcastGameEvent(string type, object gameEvent)
    {
        var jsonEvent = (JsonElement)gameEvent;
        var targetType = typeof(IGameEvent).Assembly.GetType(type);

        if (targetType is null)
        {
            _logger.LogError("Cannot find type {type}", type);
            return Task.CompletedTask;
        }

        IGameEvent? typedEvent = null;

        // If SignalR .NET client gets support for polymorphic json serialization we can
        // cange the signature of this method to IGameEvent and get rid of this mess.
        switch (targetType.Name)
        {
            case nameof(GameReset):
                typedEvent = DeserializeEvent<GameReset>(jsonEvent);
                break;
            case nameof(PlayerJoinedGame):
                typedEvent = DeserializeEvent<PlayerJoinedGame>(jsonEvent);
                break;
            case nameof(PlayerLeftGame):
                typedEvent = DeserializeEvent<PlayerLeftGame>(jsonEvent);
                break;
            case nameof(Sync):
                typedEvent = DeserializeEvent<Sync>(jsonEvent);
                break;
            case nameof(VoteCast):
                typedEvent = DeserializeEvent<VoteCast>(jsonEvent);
                break;
            case nameof(VotesShown):
                typedEvent = DeserializeEvent<VotesShown>(jsonEvent);
                break;
            default:
                _logger.LogError("Cannot handle type {type}", targetType.Name);
                break;
        }

        if (typedEvent is not null)
        {
            _logger.LogInformation("Received {type} event with: {event}", type, typedEvent);
            return Clients.All.ReceiveGameEvent(typedEvent);
        }

        return Task.CompletedTask;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected. {id}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
            _logger.LogError(exception, "Client disconnected with exception.");
        else
            _logger.LogInformation("Client disconnected gracefully.");

        var gameId = ContextGameId();
        var playerName = ContextPlayerName();

        await Clients.All.ReceiveGameEvent(new PlayerLeftGame(gameId, playerName));
        
        await base.OnDisconnectedAsync(exception);
    }
    
    private static T? DeserializeEvent<T>(JsonElement jsonEvent) where T : class, IGameEvent
    {
        return jsonEvent.Deserialize<T>(_jsonOptions);
    }

    private string ContextGameId()
    {
        if (Context.GetHttpContext() is { } ctx)
        {
            return ctx.Request.Query["gameId"]!;
        }

        return string.Empty;
    }
    
    private string ContextPlayerName()
    {
        if (Context.GetHttpContext() is { } ctx)
        {
            return ctx.Request.Query["playerName"]!;
        }

        return string.Empty;
    }
}
