namespace PointingParty.Domain.Events;

public record PlayerLeftGame(string GameId, string PlayerName) : IGameEvent;