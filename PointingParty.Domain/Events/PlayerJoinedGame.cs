namespace PointingParty.Domain.Events;

public record PlayerJoinedGame(string GameId, string PlayerName) : IGameEvent;