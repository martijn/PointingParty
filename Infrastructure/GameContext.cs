using MassTransit;
using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Infrastructure;

public sealed class GameContext : IAsyncDisposable
{
    private readonly IBus _bus;
    private readonly EventHub _hub;
    private readonly Guid _id;
    private readonly ILogger<GameContext> _logger;

    private string? _playerName;

    public GameContext(IBus bus, EventHub hub, ILogger<GameContext> logger)
    {
        _bus = bus;
        _hub = hub;
        _logger = logger;

        _id = Guid.NewGuid();

        _logger.LogDebug("GameContext {_id}: Started", _id);
    }

    public GameAggregate? Game { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (Game is not null && _playerName is not null)
            await _bus.Publish(new PlayerLeftGame(Game!.State.GameId, _playerName));

        _logger.LogDebug("GameContext {_id}: Disposing", _id);

        _hub.OnEvent -= HandleGameEvent;
    }

    public event Action<bool>? OnStateChange;

    public GameAggregate Start(string gameId, string playerName, Action<bool>? stateChangeHandler)
    {
        Game = new GameAggregate(gameId);
        _playerName = playerName;
        OnStateChange += stateChangeHandler;
        _hub.OnEvent += HandleGameEvent;

        _logger.LogDebug("GameContext {_id}: Loaded gameId {gameId} for {_playerName}", _id, gameId, _playerName);

        return Game;
    }

    public async Task PlayerJoined()
    {
        await _bus.Publish(new PlayerJoinedGame(Game!.State.GameId, _playerName!));
    }

    public async Task VoteCast(Vote vote)
    {
        await _bus.Publish(new VoteCast(Game!.State.GameId, _playerName!, vote));
    }

    public async Task ClearVotes()
    {
        await _bus.Publish(new GameReset(Game!.State.GameId));
    }

    public async Task ShowVotes()
    {
        await _bus.Publish(new VotesShown(Game!.State.GameId));
    }

    private void HandleGameEvent(IGameEvent e)
    {
        if (e.GameId != Game!.State.GameId) return;

        Game?.Handle(e);
        OnStateChange?.Invoke(e is GameReset);
    }
}
