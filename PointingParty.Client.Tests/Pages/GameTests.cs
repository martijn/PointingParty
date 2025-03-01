using Bunit.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using PointingParty.Client.Pages;
using PointingParty.Domain;

namespace PointingParty.Client.Tests.Pages;

public class GameTests : BunitContext
{
    private readonly IGameContext _gameContext;

    public GameTests()
    {
        _gameContext = Substitute.For<IGameContext>();
        Services.AddTransient<IGameContext>(_ => _gameContext);
        ComponentFactories.AddStub<GameUi>();
    }

    [Fact]
    public void WhenSubmittingForm_CreatesGame()
    {
        var cut = Render<Game>(parameters =>
            parameters.Add(p => p.GameId, "Game"));

        var nameInput = cut.Find("""input[placeholder="Player Name"]""");
        nameInput.Change("Player");

        cut.Find("button").Click();

        _gameContext.Received(1).CreateGame("Game", "Player");
        _gameContext.Received(1).Initialize(Arg.Any<Action>());
    }

    [Fact]
    public void WithGame_RendersGameUi()
    {
        _gameContext.Game.Returns(new GameAggregate());

        var cut = Render<Game>(parameters =>
            parameters.Add(p => p.GameId, "Game"));

        Assert.NotNull(cut.FindComponent<Stub<GameUi>>());
    }

    [Fact]
    public void WithPlayerName_CreatesGame_And_UpdatesUrl()
    {
        _gameContext.Game.Returns(new GameAggregate());
        var navMan = Services.GetRequiredService<BunitNavigationManager>();

        navMan.NavigateTo("/Game/Game?PlayerName=Player");

        Render<Game>(parameters => { parameters.Add(p => p.GameId, "Game"); });

        Assert.Equal("http://localhost/Game/Game", navMan.Uri);
        
        _gameContext.Received(1).CreateGame("Game", "Player");
        _gameContext.Received(1).Initialize(Arg.Any<Action>());
    }

    protected override void Dispose(bool disposing)
    {
        // Override BunitContext.Dispose to avoid calling sync Dispose on the Services
    }
}
