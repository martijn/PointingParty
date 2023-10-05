using MassTransit;

namespace PointingParty.Events;

public record VotesShown(NewId Id, string GameId) : IGameEvent;
