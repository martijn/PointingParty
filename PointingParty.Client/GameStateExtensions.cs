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
    /// Returns true only when the round is fully settled and everyone agrees:
    /// there is more than one player and every player has cast the same numeric
    /// vote. A pending vote means a player has not voted yet or their vote is
    /// still in transit, so the result is not settled and we must not celebrate
    /// prematurely (which would otherwise fire confetti for a non-unanimous
    /// round whenever a differing vote arrives just after the reveal).
    /// </summary>
    public static bool IsUnanimous(this GameState gameState)
    {
        if (gameState.PlayerVotes.Count < 2)
            return false;

        var votes = gameState.PlayerVotes.Values;
        if (votes.Any(v => v.Status != VoteStatus.Scored))
            return false;

        var first = votes.First();
        return votes.All(v => v.Equals(first));
    }
}
