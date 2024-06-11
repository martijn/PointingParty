using PointingParty.Domain.Events;

namespace PointingParty.Domain;

public interface IGameEventClient
{
    Task ReceiveGameEvent(IGameEvent gameEvent);
}
