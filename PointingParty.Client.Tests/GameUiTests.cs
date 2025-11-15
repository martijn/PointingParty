using AngleSharp.Dom;
using PointingParty.Client.Components;
using PointingParty.Domain;
using PointingParty.Domain.Events;
using Syncfusion.Blazor;

namespace PointingParty.Client.Tests;

public class GameUiTests : BunitContext
{
    private const string PlayerName = "Player";
    private const string GameId = "TestGame";
    private readonly GameAggregate _game;
    private readonly IGameContext _gameContext;

    public GameUiTests()
    {
        _game = new GameAggregate(GameId, PlayerName);
        _gameContext = Substitute.For<IGameContext>();
        _gameContext.Game = _game;

        Services.AddSyncfusionBlazor();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Renders_PlayerName()
    {
        _gameContext.PlayerName.Returns(PlayerName);

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));
        cut.Find("h3").TextContent.MarkupMatches($"Your vote, {PlayerName}:");
    }

    [Fact]
    public void Publishes_PlayerJoined_Event()
    {
        Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        Assert.Collection(_game.EventsToPublish, e => Assert.IsType<PlayerJoinedGame>(e));
        _gameContext.Received(1).PublishEvents();
    }

    [Fact]
    public void Publishes_Vote_Event()
    {
        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        _gameContext.ClearReceivedCalls();
        _game.EventsToPublish.Clear();

        cut.FindComponent<VoteButton>().Find("button").Click();

        Assert.Collection(_game.EventsToPublish, e =>
        {
            Assert.IsType<VoteCast>(e);
            Assert.Equal(((VoteCast)e).Vote, 1);
        });

        _gameContext.Received(1).PublishEvents();
    }

    [Fact]
    public void Publishes_VotesShown_Event()
    {
        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        _gameContext.ClearReceivedCalls();
        _game.EventsToPublish.Clear();

        cut.FindComponent<FatButton>().Find("button").Click();

        Assert.Collection(_game.EventsToPublish, e => { Assert.IsType<VotesShown>(e); });

        _gameContext.Received(1).PublishEvents();
    }

    [Fact]
    public void Shows_Players_Alphabetized()
    {
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        _game.Handle(new PlayerJoinedGame(GameId, "Player Three"));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        var results = cut.FindAll("""table[data-testid="results"] tbody tr td:first-child""");

        Assert.Collection(results,
            e => { Assert.Equal(PlayerName, e.GetInnerText()); },
            e => { Assert.Equal("Player Three", e.GetInnerText()); },
            e => { Assert.Equal("Player Two", e.GetInnerText()); }
        );
    }

    [Fact]
    public void Hides_Votes_By_Default()
    {
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        _game.Handle(new VoteCast(GameId, "Player Two", 8));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));
        var results = cut.Find("""[data-testid="vote-for-Player Two"]""");
        Assert.Equal("Voted!", results.GetInnerText());
    }

    [Fact]
    public void Shows_Votes_After_VotesShown_Event()
    {
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        _game.Handle(new VoteCast(GameId, "Player Two", 8));
        _game.Handle(new VotesShown(GameId));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));
        var results = cut.Find("""[data-testid="vote-for-Player Two"]""");
        Assert.Equal("8", results.GetInnerText());
    }
}
