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
    public void Takes_The_Middle_Value_As_Median()
    {
        Assert.Equal(5, StateWith(3, 5, 21).MedianVote());
    }

    [Fact]
    public void Averages_The_Two_Middle_Values_For_An_Even_Count()
    {
        Assert.Equal(4, StateWith(3, 5).MedianVote());
    }

    [Fact]
    public void Has_No_Median_Without_Scored_Votes()
    {
        var state = new GameState(
            string.Empty,
            new Dictionary<string, Vote> { { "a", VoteStatus.Coffee } }.ToImmutableDictionary(),
            true);

        Assert.Null(state.MedianVote());
        Assert.Null(state.Summarize().Median);
    }

    [Theory]
    [InlineData(Verdict.Unanimous, new double[] { 5, 5, 5 })]
    [InlineData(Verdict.Tight, new double[] { 3, 5 })] // 5/3 = 1.67
    [InlineData(Verdict.CloseEnough, new double[] { 3, 8 })] // 8/3 = 2.67
    [InlineData(Verdict.Split, new double[] { 3, 13 })] // 13/3 = 4.33
    public void Judges_The_Spread(Verdict expected, double[] votes)
    {
        Assert.Equal(expected, StateWith(votes).Summarize().Verdict);
    }

    [Fact]
    public void Judges_Only_The_Scored_Votes()
    {
        var state = new GameState(
            string.Empty,
            new Dictionary<string, Vote>
            {
                { "a", 5 },
                { "b", 5 },
                { "c", VoteStatus.Question },
                { "d", VoteStatus.Pending }
            }.ToImmutableDictionary(),
            true);

        Assert.Equal(Verdict.Unanimous, state.Summarize().Verdict);
    }

    private static GameState StateWith(params double[] votes)
    {
        return new GameState(
            string.Empty,
            votes.Select((v, i) => (Name: $"player{i}", Vote: (Vote)v))
                .ToImmutableDictionary(x => x.Name, x => x.Vote),
            true);
    }
}
