using MassTransit;
using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Infrastructure;

public sealed class GameContext : IDisposable
{
    private readonly EventHub _hub;
    private readonly Guid _id;
    private readonly ILogger<GameContext> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    private string? _playerName;

    public GameContext(IBus publishEndpoint, EventHub hub, ILogger<GameContext> logger)
    {
        _publishEndpoint = publishEndpoint;
        _hub = hub;
        _logger = logger;

        _id = Guid.NewGuid();

        _logger.LogDebug("GameContext {_id}: Started", _id);
    }

    private GameAggregate? Game { get; set; }

    public void Dispose()
    {
        if (Game is not null && _playerName is not null)
        {
            Game.PlayerLeft();
            PublishEvents();
        }

        _logger.LogDebug("GameContext {_id}: Disposing", _id);

        _hub.OnEvent -= HandleGameEvent;
    }

    public event Action? OnStateChange;

    public GameAggregate Start(string gameId, string playerName, Action? stateChangeHandler)
    {
        Game = new GameAggregate(gameId, playerName);
        _playerName = playerName;
        OnStateChange += stateChangeHandler;
        _hub.OnEvent += HandleGameEvent;

        _logger.LogDebug("GameContext {_id}: Loaded gameId {gameId} for {_playerName}", _id, gameId, _playerName);

        return Game;
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

        var publishTasks = Game.EventsToPublish.Select(gameEvent =>
            _publishEndpoint.Publish(gameEvent switch
            {
                GameReset e => e,
                PlayerJoinedGame e => e,
                PlayerLeftGame e => e,
                Sync e => e,
                VoteCast e => e,
                VotesShown e => e,
                _ => throw new NotImplementedException($"Missing PublishEvent handler for event {gameEvent}")
            })
        ).ToArray();

        Game.EventsToPublish.Clear();
        Task.WaitAll(publishTasks);
    }
}
