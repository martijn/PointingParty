namespace PointingParty.Domain.Events;

public record VoteCast(string GameId, string PlayerName, Vote Vote) : IGameEvent;
