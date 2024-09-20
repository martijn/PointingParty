using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty;

public class GameEventHub : Hub<IGameEventClient>, IGameEventHub
{
    private const string PlayerNameItem = "playerName";
    private const string GameIdItem = "gameId";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ILogger<GameEventHub> _logger;

    public GameEventHub(ILogger<GameEventHub> logger)
    {
        _logger = logger;
    }

    public async Task BroadcastGameEvent(object gameEvent)
    {
        if (gameEvent is not JsonElement jsonEvent)
        {
            _logger.LogWarning("Invalid game event received.");
            return;
        }

        var typedEvent = jsonEvent.Deserialize<IGameEvent>(JsonSerializerOptions.Web);
        if (typedEvent is null)
        {
            _logger.LogWarning("Failed to deserialize game event.");
            return;
        }
        
        if (typedEvent is PlayerJoinedGame joinedGame)
        {
            // Save player details, so we can broadcast a leave event when client disconnects
            Context.Items[PlayerNameItem] = joinedGame.PlayerName;
            Context.Items[GameIdItem] = joinedGame.GameId;
            
            // Add client to SignalR group so it receives events for the game it joined
            await Groups.AddToGroupAsync(Context.ConnectionId, joinedGame.GameId);
        }

        _logger.LogInformation("{gameId}: processing event {event}", typedEvent.GameId, typedEvent);
        await Clients.GroupExcept(typedEvent.GameId, Context.ConnectionId).ReceiveGameEvent(typedEvent);
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

        if (Context.Items[GameIdItem] is string gameId && Context.Items[PlayerNameItem] is string playerName)
        {
            _logger.LogInformation("{gameId}: Broadcasting PlayerLeftGame for {playerName}", gameId, playerName);
            await Clients.Group(gameId).ReceiveGameEvent(new PlayerLeftGame(gameId, playerName));
        }

        await base.OnDisconnectedAsync(exception);
    }
}
