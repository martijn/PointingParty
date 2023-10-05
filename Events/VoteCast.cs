using MassTransit;

namespace PointingParty.Events;

public record VoteCast(NewId Id, string GameId, string PlayerName, Vote Vote) : IGameEvent;
