using MassTransit;

namespace PointingParty.Events;

public record PlayerJoinedGame(NewId Id, string GameId, string PlayerName) : IGameEvent;
