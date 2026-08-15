using System.Globalization;
using PointingParty.Domain;

namespace PointingParty.Client;

/// <summary>
/// A round this client has watched from reveal to reset.
/// </summary>
/// <param name="Round">Round number as this client counts them.</param>
/// <param name="Median">Median of the scored votes.</param>
/// <param name="Tight">Whether the table landed close together — drives the chip's colour.</param>
/// <param name="Elapsed">How long the round took, measured to the reveal.</param>
public readonly record struct TimedRound(int Round, double Median, bool Tight, TimeSpan Elapsed);

/// <summary>
/// The session's shape so far: how long the current round has been running, and what the finished
/// ones settled on. Purely local — there is no shared clock and no persistence, so a player who
/// joins late starts with an empty timeline, exactly as they start at round one.
/// </summary>
public sealed class RoundTimeline(TimeProvider? time = null)
{
    private readonly TimeProvider _time = time ?? TimeProvider.System;
    private readonly List<TimedRound> _rounds = [];

    private int _round;
    private DateTimeOffset _started;
    private TimeSpan? _frozen;
    private RoundSummary? _revealed;

    public IReadOnlyList<TimedRound> Rounds => _rounds;

    /// <summary>
    /// Time on the current round. It stops at the reveal: the minutes spent arguing afterwards are
    /// not what the table is trying to see.
    /// </summary>
    public TimeSpan Elapsed => _frozen ?? _time.GetUtcNow() - _started;

    public string Clock => Format(Elapsed);

    /// <summary>
    /// The strip's trailing summary, e.g. "3 stories · 1:12 avg".
    /// </summary>
    public string Pace
    {
        get
        {
            if (_rounds.Count == 0) return string.Empty;

            var average = TimeSpan.FromSeconds(_rounds.Average(r => r.Elapsed.TotalSeconds));
            var stories = _rounds.Count == 1 ? "story" : "stories";
            return $"{_rounds.Count} {stories} · {Format(average)} avg";
        }
    }

    /// <summary>
    /// Folds the aggregate's current state into the timeline. Called on every render rather than
    /// hooked to an event, because a round is only finished once the reset that follows it lands —
    /// and that reset has already wiped the votes we need to record. Idempotent: it only acts on
    /// the two transitions it cares about, the reveal and the round change.
    /// </summary>
    public void Observe(GameAggregate game)
    {
        if (game.Round != _round)
        {
            if (_revealed is { Median: { } median })
                _rounds.Add(new TimedRound(_round, median, IsTight(_revealed.Verdict), _frozen ?? Elapsed));

            _revealed = null;
            _round = game.Round;
            _started = _time.GetUtcNow();
            _frozen = null;
        }

        if (!game.State.ShowVotes) return;

        _frozen ??= _time.GetUtcNow() - _started;
        _revealed = game.State.Summarize();
    }

    private static bool IsTight(Verdict verdict) => verdict is Verdict.Unanimous or Verdict.Tight;

    private static string Format(TimeSpan span) =>
        string.Create(CultureInfo.InvariantCulture, $"{(int)span.TotalMinutes}:{span.Seconds:D2}");
}
