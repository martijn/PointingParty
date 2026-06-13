using PointingParty.Domain;

namespace PointingParty.Client;

public static class GameStateExtensions
{
    public static double AverageVote(this GameState gameState)
    {
        var scoredVotes = gameState.PlayerVotes
            .Where(x => x.Value.Status == VoteStatus.Scored)
            .Select(x => x.Value.Score)
            .ToList();
        return scoredVotes.Any() ? scoredVotes.Average() : default;
    }

    /// <summary>
    /// Returns true when more than one player cast a numeric vote and every such
    /// vote is the same. Players who abstained — i.e. did not vote (Pending) or
    /// picked a non-numeric option (Coffee, Question) — are ignored.
    /// This is evaluated by the player who reveals the votes (who has the full
    /// picture) so the result is decided once and broadcast, rather than each
    /// client re-deciding against its own, possibly incomplete, state.
    /// </summary>
    public static bool IsUnanimous(this GameState gameState)
    {
        var scoredVotes = gameState.PlayerVotes.Values
            .Where(v => v.Status == VoteStatus.Scored)
            .ToList();

        return scoredVotes.Count > 1 && scoredVotes.All(v => v.Equals(scoredVotes[0]));
    }
}
