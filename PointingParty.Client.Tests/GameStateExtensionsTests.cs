using System.Collections.Immutable;
using PointingParty.Domain;

namespace PointingParty.Client.Tests;

public class GameStateExtensionsTests
{
    [Fact]
    public void Calculates_Average()
    {
        var state = new GameState(
            string.Empty,
            new Dictionary<string, Vote>
            {
                { "a", 1 },
                { "b", 2 }
            }.ToImmutableDictionary(),
            true);

        Assert.Equal(1.5, state.AverageVote());
    }

    [Theory]
    [InlineData(VoteStatus.Pending)]
    [InlineData(VoteStatus.Coffee)]
    [InlineData(VoteStatus.Question)]
    public void Ignores_Non_Voters(VoteStatus vote)
    {
        var state = new GameState(
            string.Empty,
            new Dictionary<string, Vote>
            {
                { "a", 1 },
                { "b", 2 },
                { "c", vote }
            }.ToImmutableDictionary(),
            true);

        Assert.Equal(1.5, state.AverageVote());
    }

    [Theory]
    [InlineData(VoteStatus.Pending)]
    [InlineData(VoteStatus.Coffee)]
    [InlineData(VoteStatus.Question)]
    public void Returns_Default_When_Nobody_Voted(VoteStatus vote)
    {
        var state = new GameState(
            string.Empty,
            new Dictionary<string, Vote>
            {
                { "a", vote },
                { "b", vote }
            }.ToImmutableDictionary(),
            true);

        Assert.Equal(default, state.AverageVote());
    }

    [Fact]
    public void IsUnanimous_When_Everyone_Voted_The_Same_Number()
    {
        var state = StateWith(
            ("a", 2),
            ("b", 2),
            ("c", 2));

        Assert.True(state.IsUnanimous());
    }

    [Fact]
    public void Is_Not_Unanimous_When_A_Vote_Differs()
    {
        // The scenario reported in the issue: 2, 2, 2, 1
        var state = StateWith(
            ("a", 2),
            ("b", 2),
            ("c", 2),
            ("d", 1));

        Assert.False(state.IsUnanimous());
    }

    [Theory]
    [InlineData(VoteStatus.Pending)]
    [InlineData(VoteStatus.Coffee)]
    [InlineData(VoteStatus.Question)]
    public void Is_Not_Unanimous_When_A_Player_Has_Not_Cast_A_Numeric_Vote(VoteStatus vote)
    {
        // A pending vote may still be in transit, so an apparently unanimous
        // result must not celebrate until every player has settled on a number.
        var state = StateWith(
            ("a", 2),
            ("b", 2),
            ("c", vote));

        Assert.False(state.IsUnanimous());
    }

    [Theory]
    [InlineData(VoteStatus.Pending)]
    [InlineData(VoteStatus.Coffee)]
    [InlineData(VoteStatus.Question)]
    public void Is_Not_Unanimous_When_Nobody_Cast_A_Numeric_Vote(VoteStatus vote)
    {
        var state = StateWith(
            ("a", vote),
            ("b", vote));

        Assert.False(state.IsUnanimous());
    }

    [Fact]
    public void Is_Not_Unanimous_With_A_Single_Player()
    {
        var state = StateWith(("a", 2));

        Assert.False(state.IsUnanimous());
    }

    private static GameState StateWith(params (string Player, Vote Vote)[] votes)
    {
        return new GameState(
            string.Empty,
            votes.ToImmutableDictionary(x => x.Player, x => x.Vote),
            true);
    }
}
