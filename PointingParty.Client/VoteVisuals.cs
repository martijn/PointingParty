using System.Globalization;
using PointingParty.Domain;

namespace PointingParty.Client;

/// <summary>
/// One spine on a deck card's cactus, positioned as a percentage of the mark's own box.
/// </summary>
/// <param name="X">Horizontal anchor, in percent of the mark's width.</param>
/// <param name="Y">Vertical anchor, in percent of the mark's height.</param>
/// <param name="Length">Spine length in pixels.</param>
/// <param name="Thickness">Spine thickness in pixels, to one decimal.</param>
/// <param name="Rotation">Rotation in degrees.</param>
/// <param name="Flip">-1 grows the spine leftward, 1 rightward.</param>
public readonly record struct Spine(int X, int Y, int Length, double Thickness, int Rotation, int Flip);

/// <summary>
/// The visual language of a vote: where it sits on the deck's colour ramp, and how big and how
/// prickly its cactus is. All of it is a pure function of the value, so every client draws the
/// same card.
/// </summary>
public static class VoteVisuals
{
    /// <summary>
    /// The scored values in the deck, in order. The index into this list — not the number itself —
    /// drives the ramps below, so the visual steps are even even though the values are Fibonacci.
    /// </summary>
    public static readonly double[] Scale = [1, 2, 3, 5, 8, 13, 21, 34];

    private const double Top = 7.0;

    /// <summary>
    /// The deck runs cool green to warm blue as the estimate grows: one continuous hue ramp rather
    /// than a hard split at 8. Lightness and chroma are theme tokens, so the ramp stays legible in
    /// both schemes and the whole deck reads as one family.
    /// </summary>
    public static string Tint(Vote vote) =>
        vote.Status == VoteStatus.Scored ? Tint(vote.Score) : "var(--muted)";

    public static string Tint(double score)
    {
        var index = Index(score);
        if (index < 0) return "var(--muted)";

        var hue = (int)Math.Round(158 + index / Top * 100);
        return $"oklch(var(--tint-l) var(--tint-c) {hue.ToString(CultureInfo.InvariantCulture)})";
    }

    public static int Index(double score) => Array.IndexOf(Scale, score);

    /// <summary>
    /// One asset for every value: the cactus is log-scaled by the estimate, so 1 is a sprout at
    /// 22px and 34 fills the tile at 90px. The deck gets a size gradient you can read without
    /// parsing a digit.
    /// </summary>
    public static int MarkHeight(double score) => (int)Math.Round(22 + 68 * Math.Log(score) / Math.Log(34));

    /// <summary>
    /// A fixed per-value tilt — deterministic, so a card never twitches between renders.
    /// </summary>
    public static int MarkRotation(double score) => score switch
    {
        1 => -11,
        2 => 8,
        3 => -7,
        5 => 10,
        8 => -9,
        13 => 7,
        21 => -8,
        34 => 9,
        _ => 0
    };

    /// <summary>
    /// Spine anchors in percent of the mark's box, ordered so the cactus fills out evenly as the
    /// estimate climbs: x/y trace the silhouette, with the body edges at 34%/66%, the arms at
    /// 15%/85% and the crown at 4%.
    /// </summary>
    private static readonly (int X, int Y, int Rotation, int Flip)[] Anchors =
    [
        (66, 20, -4, 1),
        (34, 30, 6, -1),
        (66, 40, 8, 1),
        (34, 15, -6, -1),
        (50, 14, -90, 1),
        (80, 22, -6, 1),
        (20, 40, 4, -1),
        (66, 52, 10, 1),
        (34, 47, 10, -1),
        (80, 33, 6, 1),
        (20, 31, -8, -1),
        (41, 15, -96, 1),
        (59, 15, -84, 1),
        (66, 10, -10, 1),
        (34, 38, 0, -1),
        (80, 14, -12, 1),
        (20, 47, 8, -1),
        (66, 30, 2, 1)
    ];

    /// <summary>
    /// A 1 is a smooth little pad; a 34 bristles. Both the count and the length of the spines climb
    /// with the estimate — the count quadratically, so the top of the deck pulls visibly away.
    /// </summary>
    public static IReadOnlyList<Spine> Spines(double score)
    {
        var index = Index(score);
        if (index < 1) return [];

        var height = MarkHeight(score);
        var t = index / Top;
        var count = (int)Math.Round(1 + 17 * t * t);
        var length = Math.Max(3, (int)Math.Round(height * (0.06 + 0.05 * t)));
        var thickness = Math.Max(1.4, Math.Round(height * 0.02, 1));

        return Anchors
            .Take(count)
            .Select(a => new Spine(a.X, a.Y, length, thickness, a.Rotation, a.Flip))
            .ToList();
    }
}
