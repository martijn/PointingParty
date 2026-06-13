namespace PointingParty.Domain.Events;

public record VotesShown(string GameId, bool ShowConfetti = false) : IGameEvent;