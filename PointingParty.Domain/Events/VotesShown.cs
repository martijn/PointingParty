namespace PointingParty.Domain.Events;

public record VotesShown(string GameId) : IGameEvent;