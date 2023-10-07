namespace PointingParty.Domain.Events;

public interface IGameEvent
{
    public string GameId { get; init; }
}
