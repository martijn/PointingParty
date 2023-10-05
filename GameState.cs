namespace PointingParty;

public record GameState(string GameId, Dictionary<string, double?> PlayerVotes, bool ShowVotes);
