using PointingParty.Domain.Events;

namespace PointingParty.Domain;

public interface IGameEventClient
{
    public Task ReceiveGameEvent(IGameEvent gameEvent);
}