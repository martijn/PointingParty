using System.Collections.Immutable;

namespace PointingParty.Domain;

public record GameState(string GameId, ImmutableDictionary<string, Vote> PlayerVotes, bool ShowVotes);
