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
            case PlayerJoinedGame playerJoinedGame:
                Apply(playerJoinedGame);
                if (playerJoinedGame.PlayerName != _playerName)
                    EventsToPublish.Add(new Sync(State.GameId, _playerName, CurrentVote));
                break;
            case PlayerLeftGame playerLeftGame:
                Apply(playerLeftGame);
                break;
            case GameReset reset:
                CurrentVote = new Vote();
                Apply(reset);
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

    private void Apply(PlayerJoinedGame e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.SetItem(e.PlayerName, new Vote())
        };
    }

    private void Apply(PlayerLeftGame e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.Remove(e.PlayerName)
        };
    }

    private void Apply(GameReset e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.ToImmutableDictionary(pv => pv.Key, _ => new Vote(VoteStatus.Pending)),
            ShowVotes = false
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

    public void PlayerJoined()
    {
        EventsToPublish.Add(new PlayerJoinedGame(State.GameId, _playerName));
    }

    public void VoteCast(Vote vote)
    {
        CurrentVote = vote;
        EventsToPublish.Add(new VoteCast(State.GameId, _playerName, vote));
    }

    public void ClearVotes()
    {
        CurrentVote = new Vote();
        EventsToPublish.Add(new GameReset(State.GameId));
    }

    public void ShowVotes()
    {
        EventsToPublish.Add(new VotesShown(State.GameId));
    }
}
