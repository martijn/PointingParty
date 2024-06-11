namespace PointingParty.Domain.Events;

public record Sync(string GameId, string PlayerName, Vote Vote) : IGameEvent;