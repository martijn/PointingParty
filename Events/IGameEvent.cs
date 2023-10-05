using MassTransit;

namespace PointingParty.Events;

public interface IGameEvent
{
    public NewId Id { get; init; }
    public string GameId { get; init; }
}
