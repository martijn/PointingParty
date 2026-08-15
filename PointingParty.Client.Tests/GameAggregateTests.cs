using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Client.Tests;

public class GameAggregateTests
{
    private const string GameId = "TestGame";

    [Fact]
    public void Starts_At_Round_One()
    {
        Assert.Equal(1, new GameAggregate(GameId, "Player").Round);
    }

    [Fact]
    public void Counts_A_Round_Per_Reset()
    {
        var game = new GameAggregate(GameId, "Player");

        game.Handle(new GameReset(GameId));
        game.Handle(new GameReset(GameId));

        Assert.Equal(3, game.Round);
    }

    [Fact]
    public void Counts_Rounds_Started_Locally_Too()
    {
        var game = new GameAggregate(GameId, "Player");

        game.GameReset();

        Assert.Equal(2, game.Round);
    }

    [Fact]
    public void Does_Not_Count_A_Round_When_Resyncing_After_A_Reconnect()
    {
        var game = new GameAggregate(GameId, "Player");
        game.VoteCast(5);

        game.Resync();

        Assert.Equal(1, game.Round);
        Assert.Equal(VoteStatus.Pending, game.CurrentVote.Status);
    }

    [Fact]
    public void Clears_The_Table_When_Resyncing()
    {
        var game = new GameAggregate(GameId, "Player");
        game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        game.Handle(new VoteCast(GameId, "Player Two", 8));
        game.Handle(new VotesShown(GameId));

        game.Resync();

        Assert.False(game.State.ShowVotes);
        Assert.All(game.State.PlayerVotes.Values, v => Assert.Equal(VoteStatus.Pending, v.Status));
    }
}
