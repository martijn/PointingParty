using MassTransit;

namespace PointingParty.Domain.Events;

public record VotesShown(NewId Id, string GameId) : IGameEvent;
