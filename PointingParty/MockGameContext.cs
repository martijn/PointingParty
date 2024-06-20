using PointingParty.Client;
using PointingParty.Domain;

namespace PointingParty;

// A mock GameContext to allow server-side prerendering of the Game page
public class MockGameContext : IGameContext
{
    public GameAggregate? Game { get; set; }
    public string? PlayerName { get; set; }
    public ConnectionStatus Status => ConnectionStatus.Connecting;

    public Task Initialize(Action? stateChangeHandler)
    {
        return Task.CompletedTask;
    }

    public GameAggregate CreateGame(string gameId, string playerName)
    {
        return new GameAggregate();
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void PublishEvents()
    {
    }
}
