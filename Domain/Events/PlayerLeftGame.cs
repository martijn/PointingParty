using MassTransit;

namespace PointingParty.Domain.Events;

public record PlayerLeftGame(NewId Id, string GameId, string PlayerName) : IGameEvent;
