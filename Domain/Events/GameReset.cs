using MassTransit;

namespace PointingParty.Domain.Events;

public record GameReset(NewId Id, string GameId) : IGameEvent;
