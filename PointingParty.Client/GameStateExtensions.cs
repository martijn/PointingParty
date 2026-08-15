using PointingParty.Domain;

namespace PointingParty.Client;

/// <summary>
/// How far apart the table landed, once the votes are on it.
/// </summary>
public enum Verdict
{
    Unanimous,
    Tight,
    CloseEnough,
    Split
}

/// <summary>
/// Everything the reveal panel needs about a finished round. Abstentions (question mark
/// and coffee) are excluded throughout: they carry no size information.
/// </summary>
/// <param name="Verdict">How far apart the scored votes are.</param>
/// <param name="Note">One line of plain-language guidance for the table.</param>
/// <param name="Median">Median of the scored votes, or null when nobody put a number down.</param>
public record RoundSummary(Verdict Verdict, string Note, double? Median);

public static class GameStateExtensions
{
    public static double AverageVote(this GameState gameState)
    {
        var scoredVotes = gameState.ScoredVotes();
        return scoredVotes.Any() ? scoredVotes.Average() : default;
    }

    public static double? MedianVote(this GameState gameState)
    {
        var scores = gameState.ScoredVotes();
        if (scores.Count == 0) return null;

        var middle = scores.Count / 2;
        return scores.Count % 2 == 1
            ? scores[middle]
            : (scores[middle - 1] + scores[middle]) / 2;
    }

    public static RoundSummary Summarize(this GameState gameState)
    {
        var scores = gameState.ScoredVotes();
        var median = gameState.MedianVote();

        if (scores.Count == 0)
            return new RoundSummary(Verdict.Split, "Nobody put a number down — no estimate to take from this round.", null);

        var low = scores[0];
        var high = scores[^1];
        var distinct = scores.Distinct().Count();

        // Ratios rather than absolute gaps: 1 vs 2 is the same disagreement as 13 vs 21.
        var spread = low > 0 ? high / low : double.PositiveInfinity;
        var verdict = distinct switch
        {
            1 => Verdict.Unanimous,
            _ when spread <= 1.7 => Verdict.Tight,
            _ when spread <= 3 => Verdict.CloseEnough,
            _ => Verdict.Split
        };

        var note = distinct == 1
            ? $"Everyone landed on {low}. Ship the estimate and move on."
            : $"Spread {low}–{high} across {scores.Count} estimates. Let the outliers make their case.";

        return new RoundSummary(verdict, note, median);
    }

    /// <summary>
    /// The scored votes only, in ascending order.
    /// </summary>
    private static List<double> ScoredVotes(this GameState gameState)
    {
        return gameState.PlayerVotes
            .Where(x => x.Value.Status == VoteStatus.Scored)
            .Select(x => x.Value.Score)
            .Order()
            .ToList();
    }
}
