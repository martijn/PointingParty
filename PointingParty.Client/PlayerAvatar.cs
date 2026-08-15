namespace PointingParty.Client;

/// <summary>
/// Avatar colours derived from the player's name alone. Every client renders the roster from
/// its own event log, so the colour has to be a pure function of the name — the same player
/// must come out the same colour everywhere, with no server state and no randomness.
/// </summary>
public static class PlayerAvatar
{
    /// <summary>
    /// FNV-1a over the trimmed, lower-cased name, spread by the golden angle so that
    /// near-identical names ("Martijn" / "Martin") land far apart on the colour wheel.
    /// </summary>
    /// <remarks>
    /// Only the hue comes from the name. Lightness and chroma are theme tokens, which is what
    /// keeps six avatars in a row looking like one family and keeps the initials legible.
    /// </remarks>
    public static int Hue(string name)
    {
        var key = name.Trim().ToLowerInvariant();
        uint hash = 2166136261;
        foreach (var ch in key)
        {
            hash ^= ch;
            hash *= 16777619;
        }

        return (int)Math.Round(hash * 137.508 % 360);
    }

    /// <summary>
    /// First two characters of the name, upper-cased.
    /// </summary>
    public static string Initials(string name)
    {
        var key = name.Trim();
        return key.Length == 0 ? "?" : key[..Math.Min(2, key.Length)].ToUpperInvariant();
    }
}
