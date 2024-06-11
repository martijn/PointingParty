using System.Collections.Immutable;
using PointingParty.Domain.Events;

namespace PointingParty.Domain;

public class GameAggregate
{
    private readonly string _playerName;

    public GameAggregate(string gameId, string playerName)
    {
        State = new GameState(gameId, ImmutableDictionary<string, Vote>.Empty, false);
        _playerName = playerName;
    }

    public List<IGameEvent> EventsToPublish { get; set; } = new();

    public GameState State { get; private set; }
    public Vote CurrentVote { get; private set; }

    public void Handle(IGameEvent eventMessage)
    {
        switch (eventMessage)
        {
            case GameReset reset:
                Apply(reset);
                break;
            case PlayerJoinedGame playerJoinedGame:
                Apply(playerJoinedGame);
                break;
            case PlayerLeftGame playerLeftGame:
                Apply(playerLeftGame);
                break;
            case Sync sync:
                Apply(sync);
                break;
            case VoteCast voteCast:
                Apply(voteCast);
                break;
            case VotesShown votesShown:
                Apply(votesShown);
                break;
            default:
                throw new NotImplementedException($"Missing handler for {eventMessage}");
        }
    }

    private void Apply(GameReset e)
    {
        CurrentVote = new Vote();

        State = State with
        {
            PlayerVotes = State.PlayerVotes.ToImmutableDictionary(pv => pv.Key, _ => new Vote(VoteStatus.Pending)),
            ShowVotes = false
        };
    }

    private void Apply(PlayerJoinedGame e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.SetItem(e.PlayerName, new Vote())
        };

        if (e.PlayerName != _playerName)
            EventsToPublish.Add(new Sync(State.GameId, _playerName, CurrentVote));
    }

    private void Apply(PlayerLeftGame e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.Remove(e.PlayerName)
        };
    }


    private void Apply(Sync e)
    {
        if (State.PlayerVotes.ContainsKey(e.PlayerName)) return;

        State = State with
        {
            PlayerVotes = State.PlayerVotes.SetItem(e.PlayerName, e.Vote)
        };
    }

    private void Apply(VoteCast e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.SetItem(e.PlayerName, e.Vote)
        };
    }

    private void Apply(VotesShown e)
    {
        State = State with { ShowVotes = true };
    }

    public void GameReset()
    {
        var clearVotesEvent = new GameReset(State.GameId);
        Apply(clearVotesEvent);
        EventsToPublish.Add(clearVotesEvent);
    }

    public void PlayerJoined()
    {
        var playerJoinedEvent = new PlayerJoinedGame(State.GameId, _playerName);
        Apply(playerJoinedEvent);
        EventsToPublish.Add(playerJoinedEvent);
    }

    public void PlayerLeft()
    {
        var playerLeftEvent = new PlayerLeftGame(State.GameId, _playerName);
        Apply(playerLeftEvent);
        EventsToPublish.Add(playerLeftEvent);
    }

    public void VoteCast(Vote vote)
    {
        CurrentVote = vote;
        var voteCastEvent = new VoteCast(State.GameId, _playerName, vote);
        Apply(voteCastEvent);
        EventsToPublish.Add(voteCastEvent);
    }

    public void VotesShown()
    {
        var showVotesEvent = new VotesShown(State.GameId);
        Apply(showVotesEvent);
        EventsToPublish.Add(showVotesEvent);
    }
}