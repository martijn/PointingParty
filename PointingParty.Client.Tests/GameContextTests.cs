using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace PointingParty.Client.Tests;

public class GameContextTests
{
    [Fact]
    public void CreateGame_Returns_GameAggregate()
    {
        var sut = new GameContext(
            Substitute.For<ILogger<GameContext>>(),
            Substitute.For<NavigationManager>()
        );

        var agg = sut.CreateGame("Game", "Player");

        Assert.Equal(sut.Game, agg);
        Assert.Equal("Game", agg.State.GameId);
        Assert.Equal("Player", sut.PlayerName);
    }
}
