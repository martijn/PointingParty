namespace PointingParty.Domain;

public interface IGameEventHub
{
    // SignalR client doesn't support polymorphic serialization yet, so we send a type + object
    public Task BroadcastGameEvent(string type, object gameEvent);
}
