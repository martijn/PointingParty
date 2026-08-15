using AngleSharp.Dom;
using PointingParty.Client.Components;
using PointingParty.Domain;
using PointingParty.Domain.Events;

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

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Marks_The_Local_Player()
    {
        _gameContext.PlayerName.Returns(PlayerName);

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        var me = cut.Find($"""[data-testid="player-row-{PlayerName}"] .pp-player-name""");
        Assert.Equal($"{PlayerName} (you)", me.GetInnerText());
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

        cut.FindComponent<VoteCard>().Find("button").Click();

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
    public void Shows_Local_Player_First_Then_Alphabetized()
    {
        _gameContext.PlayerName.Returns("Player Two");
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        _game.Handle(new PlayerJoinedGame(GameId, "Player Three"));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        var results = cut.FindAll("""[data-testid="results"] .pp-player-name""");

        Assert.Collection(results,
            e => { Assert.Equal("Player Two (you)", e.GetInnerText()); },
            e => { Assert.Equal(PlayerName, e.GetInnerText()); },
            e => { Assert.Equal("Player Three", e.GetInnerText()); }
        );
    }

    [Fact]
    public void Hides_Votes_By_Default()
    {
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        _game.Handle(new VoteCast(GameId, "Player Two", 8));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        Assert.Equal("", cut.Find("""[data-testid="vote-for-Player Two"]""").GetInnerText());
        Assert.Equal("ready", cut.Find("""[data-testid="state-for-Player Two"]""").GetInnerText());
    }

    [Fact]
    public void Marks_Players_Who_Have_Not_Voted_As_Thinking()
    {
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        Assert.Equal("thinking…", cut.Find("""[data-testid="state-for-Player Two"]""").GetInnerText());
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

    [Fact]
    public void Shows_Verdict_And_Median_After_Reveal()
    {
        _game.Handle(new PlayerJoinedGame(GameId, "Player Two"));
        _game.Handle(new PlayerJoinedGame(GameId, "Player Three"));
        _game.Handle(new VoteCast(GameId, "Player Two", 3));
        _game.Handle(new VoteCast(GameId, "Player Three", 5));
        _game.Handle(new VotesShown(GameId));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        Assert.Equal("Tight", cut.Find("""[data-testid="verdict"]""").GetInnerText());
        Assert.Equal("4", cut.Find("""[data-testid="median"]""").GetInnerText());
    }

    [Fact]
    public void Hides_The_Reveal_Panel_Until_Votes_Are_Shown()
    {
        _game.Handle(new VoteCast(GameId, PlayerName, 3));

        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        Assert.Empty(cut.FindAll("""[data-testid="verdict"]"""));
    }

    [Fact]
    public void Offers_A_Card_For_Every_Deck_Value()
    {
        var cut = Render<GameUi>(parameters => parameters.Add(p => p.GameContext, _gameContext));

        Assert.Equal(10, cut.FindComponents<VoteCard>().Count);
    }
}
