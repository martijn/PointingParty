using PointingParty.Events;

namespace PointingParty;

public class GameAggregate
{
    public GameAggregate(string gameId)
    {
        State = new GameState(gameId, new Dictionary<string, double?>(), false);
    }

    public GameState State { get; private set; }

    public void Handle(IGameEvent eventMessage)
    {
        switch (eventMessage)
        {
            case PlayerJoinedGame playerJoinedGame:
                Apply(playerJoinedGame);
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
        newPlayerVotes[e.PlayerName] = null;

        State = State with { PlayerVotes = newPlayerVotes };
    }

    private void Apply(GameReset e)
    {
        State = State with
        {
            PlayerVotes = State.PlayerVotes.ToDictionary(pv => pv.Key, _ => (double?)null),
            ShowVotes = false
        };
    }

    private void Apply(VoteCast e)
    {
        var newPlayerVotes = State.PlayerVotes;
        newPlayerVotes[e.PlayerName] = e.Score;
        State = State with { PlayerVotes = newPlayerVotes };
    }

    private void Apply(VotesShown e)
    {
        State = State with { ShowVotes = true };
    }
}
