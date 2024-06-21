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
}
