using PointingParty.Domain;

namespace PointingParty.Client.Tests;

public class VoteVisualsTests
{
    [Fact]
    public void Walks_The_Hue_Ramp_From_Green_To_Blue()
    {
        // The deck is one continuous ramp rather than a hard split at 8, so the endpoints are
        // fixed and everything in between only ever climbs.
        Assert.Equal("oklch(var(--tint-l) var(--tint-c) 158)", VoteVisuals.Tint(1));
        Assert.Equal("oklch(var(--tint-l) var(--tint-c) 258)", VoteVisuals.Tint(34));
    }

    [Fact]
    public void Leaves_The_Ramp_To_The_Abstentions()
    {
        Assert.Equal("var(--muted)", VoteVisuals.Tint(new Vote(VoteStatus.Question)));
        Assert.Equal("var(--muted)", VoteVisuals.Tint(new Vote(VoteStatus.Coffee)));
        Assert.Equal("var(--muted)", VoteVisuals.Tint(new Vote()));
        Assert.Equal("var(--muted)", VoteVisuals.Tint(7));
    }

    [Fact]
    public void Grows_The_Cactus_With_The_Estimate()
    {
        var heights = VoteVisuals.Scale.Select(VoteVisuals.MarkHeight).ToList();

        Assert.Equal(22, heights[0]);
        Assert.Equal(90, heights[^1]);
        Assert.Equal(heights.Order(), heights);
    }

    [Fact]
    public void Bristles_Only_As_The_Estimate_Climbs()
    {
        var counts = VoteVisuals.Scale.Select(v => VoteVisuals.Spines(v).Count).ToList();

        // A 1 is a smooth little pad; a 34 carries the full set.
        Assert.Equal(0, counts[0]);
        Assert.Equal(18, counts[^1]);
        Assert.Equal(counts.Order(), counts);
    }

    [Fact]
    public void Keeps_Spines_Inside_The_Marks_Box()
    {
        foreach (var spine in VoteVisuals.Scale.SelectMany(VoteVisuals.Spines))
        {
            Assert.InRange(spine.X, 0, 100);
            Assert.InRange(spine.Y, 0, 100);
            Assert.True(spine.Length >= 3);
            Assert.True(spine.Thickness >= 1.4);
        }
    }
}
