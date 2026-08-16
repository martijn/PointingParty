using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Client.Tests;

/// <summary>
/// A clock that only moves when the test says so.
/// </summary>
internal sealed class StoppedClock : TimeProvider
{
    private DateTimeOffset _now = new(2026, 1, 1, 9, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow() => _now;

    public void Advance(TimeSpan by) => _now += by;
}

public class RoundTimelineTests
{
    private const string GameId = "TestGame";

    private readonly StoppedClock _time = new();
    private readonly GameAggregate _game = new(GameId, "Player One");
    private readonly RoundTimeline _timeline;

    public RoundTimelineTests()
    {
        _timeline = new RoundTimeline(_time);
        _game.Handle(new PlayerJoinedGame(GameId, "Player One"));
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
    }

    private void Advance(int seconds) => _time.Advance(TimeSpan.FromSeconds(seconds));

    [Fact]
    public void Starts_Empty()
    {
        _timeline.Observe(_game);

        Assert.Empty(_timeline.Rounds);
        Assert.Equal("0:00", _timeline.Clock);
        Assert.Equal("", _timeline.Pace);
    }

    [Fact]
    public void Runs_The_Clock_On_The_Open_Round()
    {
        _timeline.Observe(_game);
        Advance(75);

        Assert.Equal("1:15", _timeline.Clock);
    }

    [Fact]
    public void Stops_The_Clock_At_The_Reveal()
    {
        _timeline.Observe(_game);
        Advance(20);

        _game.Handle(new VotesShown(GameId));
        _timeline.Observe(_game);

        // The minutes spent arguing after the reveal are not what the table is trying to see.
        Advance(600);
        _timeline.Observe(_game);

        Assert.Equal("0:20", _timeline.Clock);
    }

    [Fact]
    public void Records_A_Round_Once_The_Reset_Closes_It()
    {
        _timeline.Observe(_game);
        _game.Handle(new VoteCast(GameId, "Player One", 5));
        _game.Handle(new VoteCast(GameId, "Player Two", 8));
        Advance(45);
        _game.Handle(new VotesShown(GameId));
        _timeline.Observe(_game);

        // Still open until the table moves on.
        Assert.Empty(_timeline.Rounds);

        _game.Handle(new GameReset(GameId));
        _timeline.Observe(_game);

        var round = Assert.Single(_timeline.Rounds);
        Assert.Equal(1, round.Round);
        Assert.Equal(6.5, round.Median);
        Assert.Equal(Verdict.Tight, round.Verdict);
        Assert.Equal(TimeSpan.FromSeconds(45), round.Elapsed);
        Assert.Equal("1 story · 0:45 avg", _timeline.Pace);
        Assert.Equal("0:00", _timeline.Clock);
    }

    [Fact]
    public void Records_A_Round_Only_One_Player_Scored_As_Inconclusive()
    {
        _timeline.Observe(_game);
        _game.Handle(new VoteCast(GameId, "Player One", 5));
        _game.Handle(new VoteCast(GameId, "Player Two", VoteStatus.Question));
        _game.Handle(new VotesShown(GameId));
        _timeline.Observe(_game);
        _game.Handle(new GameReset(GameId));
        _timeline.Observe(_game);

        var round = Assert.Single(_timeline.Rounds);
        Assert.Equal(Verdict.Inconclusive, round.Verdict);
    }

    [Fact]
    public void Skips_A_Round_Nobody_Put_A_Number_On()
    {
        _timeline.Observe(_game);
        _game.Handle(new VoteCast(GameId, "Player One", VoteStatus.Coffee));
        _game.Handle(new VotesShown(GameId));
        _timeline.Observe(_game);
        _game.Handle(new GameReset(GameId));
        _timeline.Observe(_game);

        Assert.Empty(_timeline.Rounds);
    }

    [Fact]
    public void Skips_A_Round_That_Was_Reset_Before_The_Reveal()
    {
        _timeline.Observe(_game);
        _game.Handle(new VoteCast(GameId, "Player One", 5));
        _game.Handle(new GameReset(GameId));
        _timeline.Observe(_game);

        Assert.Empty(_timeline.Rounds);
    }

    [Fact]
    public void Averages_The_Rounds_It_Has_Seen()
    {
        foreach (var seconds in new[] { 30, 90 })
        {
            _timeline.Observe(_game);
            _game.Handle(new VoteCast(GameId, "Player One", 3));
            Advance(seconds);
            _game.Handle(new VotesShown(GameId));
            _timeline.Observe(_game);
            _game.Handle(new GameReset(GameId));
            _timeline.Observe(_game);
        }

        Assert.Equal(2, _timeline.Rounds.Count);
        Assert.Equal("2 stories · 1:00 avg", _timeline.Pace);
    }

    [Fact]
    public void Is_Safe_To_Call_On_Every_Render()
    {
        _timeline.Observe(_game);
        _game.Handle(new VoteCast(GameId, "Player One", 3));
        _game.Handle(new VotesShown(GameId));

        for (var i = 0; i < 5; i++) _timeline.Observe(_game);

        _game.Handle(new GameReset(GameId));

        for (var i = 0; i < 5; i++) _timeline.Observe(_game);

        Assert.Single(_timeline.Rounds);
    }
}
