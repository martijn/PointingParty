using MassTransit;

namespace PointingParty.Events;

public record GameReset(NewId Id, string GameId) : IGameEvent;
