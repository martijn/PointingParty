namespace PointingParty.Client.Tests;

public class PlayerAvatarTests
{
    [Fact]
    public void Gives_The_Same_Name_The_Same_Hue()
    {
        Assert.Equal(PlayerAvatar.Hue("Martijn"), PlayerAvatar.Hue("Martijn"));
    }

    [Theory]
    [InlineData("Martijn", " martijn ")]
    [InlineData("Martijn", "MARTIJN")]
    public void Ignores_Casing_And_Surrounding_Space(string one, string other)
    {
        Assert.Equal(PlayerAvatar.Hue(one), PlayerAvatar.Hue(other));
    }

    [Fact]
    public void Spreads_Near_Identical_Names_Apart()
    {
        // The golden-angle step is the whole point: without it "Martijn" and "Martin" would
        // land next to each other and be indistinguishable in the roster.
        var distance = Math.Abs(PlayerAvatar.Hue("Martijn") - PlayerAvatar.Hue("Martin"));
        Assert.True(Math.Min(distance, 360 - distance) > 20);
    }

    [Fact]
    public void Stays_On_The_Colour_Wheel()
    {
        foreach (var name in new[] { "a", "Zoë", "", "   ", "a very long player name indeed" })
        {
            var hue = PlayerAvatar.Hue(name);
            Assert.InRange(hue, 0, 360);
        }
    }

    [Theory]
    [InlineData("Martijn", "MA")]
    [InlineData(" bram", "BR")]
    [InlineData("A", "A")]
    [InlineData("   ", "?")]
    public void Labels_With_The_First_Two_Characters(string name, string expected)
    {
        Assert.Equal(expected, PlayerAvatar.Initials(name));
    }
}
