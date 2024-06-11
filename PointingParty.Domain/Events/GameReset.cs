namespace PointingParty.Domain.Events;

public record GameReset(string GameId) : IGameEvent;