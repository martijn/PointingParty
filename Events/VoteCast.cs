using MassTransit;

namespace PointingParty.Events;

public record VoteCast(NewId Id, string GameId, string PlayerName, double Score) : IGameEvent;
