using MassTransit;
using PointingParty.Events;

namespace PointingParty;

public sealed class GameContext : IAsyncDisposable
{
    private readonly IBus _bus;
    private readonly EventHub _hub;
    private readonly ILogger<GameContext> _logger;
    private readonly Guid _id;

    private string? _playerName;
    
    public GameAggregate? Game { get; set; }

    public event Action<bool>? OnStateChange;
    
    public GameContext(IBus bus, EventHub hub, ILogger<GameContext> logger)
    {
        _bus = bus;
        _hub = hub;
        _logger = logger;

        _id = Guid.NewGuid();
        
        _logger.LogDebug("GameContext {_id}: Started", _id);
    }

    public async Task Start(string gameId, string playerName)
    {
        _playerName = playerName;
        Game = new GameAggregate(gameId);
        _hub.OnEvent += HandleGameEvent; // TODO dispose + leave event
        
        _logger.LogDebug("GameContext {_id}: Loaded gameId {gameId} for {_playerName}", _id, gameId, _playerName);
    }

    public async Task PlayerJoined()
    {
        await _bus.Publish(new PlayerJoinedGame(NewId.Next(), Game!.State.GameId, _playerName));
    }

    public async Task VoteCast(Vote vote)
    {
        await _bus.Publish(new VoteCast(NewId.Next(), Game!.State.GameId, _playerName, vote));
    }

    public async Task ClearVotes()
    {
        await _bus.Publish(new GameReset(NewId.Next(), Game!.State.GameId));
    }
    
    public async Task ShowVotes()
    {
        await _bus.Publish(new VotesShown(NewId.Next(), Game!.State.GameId));
    }

    public async ValueTask DisposeAsync()
    {
        if (Game is not null && _playerName is not null)
            await _bus.Publish(new PlayerLeftGame(NewId.Next(), Game!.State.GameId, _playerName));
        
        _logger.LogDebug("GameContext {_id}: Disposing", _id);
    }
    
    private void HandleGameEvent(IGameEvent e)
    {
        if (e.GameId != Game!.State.GameId) return;

        Game?.Handle(e);
        OnStateChange?.Invoke(e is GameReset);
    }
}
