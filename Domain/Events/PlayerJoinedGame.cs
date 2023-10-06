using MassTransit;

namespace PointingParty.Domain.Events;

public record PlayerJoinedGame(NewId Id, string GameId, string PlayerName) : IGameEvent;
