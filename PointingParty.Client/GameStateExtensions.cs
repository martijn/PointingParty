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
}
