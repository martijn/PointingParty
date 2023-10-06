using MassTransit;

namespace PointingParty.Domain.Events;

public record VoteCast(NewId Id, string GameId, string PlayerName, Vote Vote) : IGameEvent;
