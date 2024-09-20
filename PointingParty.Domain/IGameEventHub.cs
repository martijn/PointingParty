namespace PointingParty.Domain;

public interface IGameEventHub
{
    public Task BroadcastGameEvent(object gameEvent);
}
