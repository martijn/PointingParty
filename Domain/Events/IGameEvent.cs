using MassTransit;

namespace PointingParty.Domain.Events;

public interface IGameEvent
{
    public NewId Id { get; init; }
    public string GameId { get; init; }
}
