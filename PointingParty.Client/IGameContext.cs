using PointingParty.Domain;

namespace PointingParty.Client;

public interface IGameContext : IAsyncDisposable
{
    public GameAggregate? Game { get; set; }
    string? PlayerName { get; set; }
    ConnectionStatus Status { get; }
    Task Initialize(Action? stateChangeHandler);
    GameAggregate CreateGame(string gameId, string playerName);
    void PublishEvents();
}
