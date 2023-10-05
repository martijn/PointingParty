using MassTransit;

namespace PointingParty.Events;

public record PlayerLeftGame(NewId Id, string GameId, string PlayerName) : IGameEvent;
