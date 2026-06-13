using PointingParty.Domain;
using PointingParty.Domain.Events;

namespace PointingParty.Client.Tests;

public class GameAggregateTests
{
    private const string GameId = "TestGame";
    private const string PlayerName = "Player";

    [Fact]
    public void Votes_Cast_After_Reveal_Are_Dropped()
    {
        var game = new GameAggregate(GameId, PlayerName);
        game.Handle(new PlayerJoinedGame(GameId, "Other"));
        game.Handle(new VoteCast(GameId, "Other", 2));
        game.Handle(new VotesShown(GameId));

        // A vote that arrives after the round was revealed must not change it.
        game.Handle(new VoteCast(GameId, "Other", 5));

        Assert.Equal(new Vote(2), game.State.PlayerVotes["Other"]);
    }

    [Fact]
    public void VotesShown_Carries_ShowConfetti_Flag()
    {
        var game = new GameAggregate(GameId, PlayerName);

        game.Handle(new VotesShown(GameId, ShowConfetti: true));

        Assert.True(game.State.ShowVotes);
        Assert.True(game.State.ShowConfetti);
    }

    [Fact]
    public void Reset_Clears_ShowConfetti()
    {
        var game = new GameAggregate(GameId, PlayerName);
        game.Handle(new VotesShown(GameId, ShowConfetti: true));

        game.Handle(new GameReset(GameId));

        Assert.False(game.State.ShowVotes);
        Assert.False(game.State.ShowConfetti);
    }
}
