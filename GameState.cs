namespace PointingParty;

public record GameState(string GameId, Dictionary<string, Vote> PlayerVotes, bool ShowVotes);
