using PointingParty.Events;

namespace PointingParty;

public class GameAggregate
{
    public GameAggregate(string gameId)
    {
        State = new GameState(gameId, new Dictionary<string, Vote>(), false);
    }

    public GameState State { get; private set; }

    public void Handle(IGameEvent eventMessage)
    {
        switch (eventMessage)
        {
            case PlayerJoinedGame playerJoinedGame:
                Apply(playerJoinedGame);
                break;
            case PlayerLeftGame playerLeftGame:
                Apply(playerLeftGame);
                break;
            case GameReset reset:
                Apply(reset);
                break;
            case VoteCast voteCast:
                Apply(voteCast);
                break;
            case VotesShown votesShown:
                Apply(votesShown);
                break;
        }
    }

    private void Apply(PlayerJoinedGame e)
    {
        // TODO use C#12 dictionary literal when available
        var newPlayerVotes = State.PlayerVotes;
        newPlayerVotes[e.PlayerName] = new Vote();

        State = State with { PlayerVotes = newPlayerVotes };
    }
    
    private void Apply(PlayerLeftGame e)
    {
        // TODO use C#12 dictionary literal when available
        var newPlayerVotes = State.PlayerVotes;
        newPlayerVotes.Remove(e.PlayerName);

        State = State with { PlayerVotes = newPlayerVotes };
    }

    private void Apply(GameReset e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.ToDictionary(pv => pv.Key, _ => new Vote(VoteStatus.Pending)),
            ShowVotes = false
        };
    }

    private void Apply(VoteCast e)
    {
        var newPlayerVotes = State.PlayerVotes;
        newPlayerVotes[e.PlayerName] = e.Vote;
        State = State with { PlayerVotes = newPlayerVotes };
    }

    private void Apply(VotesShown e)
    {
        State = State with { ShowVotes = true };
    }
}
