using System.Text.Json.Serialization;

namespace PointingParty.Domain.Events;

// Polymorphic JSON configuration is necessary for SignalR
[JsonPolymorphic]
[JsonDerivedType(typeof(GameReset), nameof(GameReset))]
[JsonDerivedType(typeof(PlayerJoinedGame), nameof(PlayerJoinedGame))]
[JsonDerivedType(typeof(PlayerLeftGame), nameof(PlayerLeftGame))]
[JsonDerivedType(typeof(Sync), nameof(Sync))]
[JsonDerivedType(typeof(VoteCast), nameof(VoteCast))]
[JsonDerivedType(typeof(VotesShown), nameof(VotesShown))]
public interface IGameEvent
{
    public string GameId { get; init; }
}