namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class StringExtensions
{
    /// <summary>
    /// Splits the string by the specified string separator, with optional StringSplitOptions.
    /// </summary>
    public static string[] Split(this string str, string separator,
        StringSplitOptions options = StringSplitOptions.None)
    {
        if (str == null)
            throw new ArgumentNullException(nameof(str));
        if (separator == null)
            throw new ArgumentNullException(nameof(separator));

        var result = new List<string>();
        int start = 0;
        int index;

        while ((index = str.IndexOf(separator, start, StringComparison.Ordinal)) != -1)
        {
            var segment = str.Substring(start, index - start);
            if (options == StringSplitOptions.None || !string.IsNullOrEmpty(segment))
                result.Add(segment);
            start = index + separator.Length;
        }

        // Add the final segment
        var lastSegment = str.Substring(start);
        if (options == StringSplitOptions.None || !string.IsNullOrEmpty(lastSegment))
            result.Add(lastSegment);

        return result.ToArray();
    }

    public static StringCursor PullString(this string? origin, string fallback, out string result, params char[] customDelimiters)
    {
        var sc = new StringCursor(origin ?? "", 0, customDelimiters);
        return sc.PullString(fallback, out result);
    }

    public static StringCursor PullInt(this string? origin, int fallback, out int result, params char[] customDelimiters)
    {
        var sc = new StringCursor(origin ?? "", 0, customDelimiters);
        return sc.PullInt(fallback, out result);
    }
}

public class StringCursor
{
    public readonly string Origin;
    private int Position, Anchor;
    private string Intermediary;
    private readonly char[] CustomDelimiters;
    public char Current => Origin.ElementAtOrDefault(Position);
    public bool AtDelimiter =>
        CustomDelimiters.Length == 0 ? char.IsWhiteSpace(Current) : CustomDelimiters.Contains(Current);
    public StringCursor(string origin, int i, char[] customDelimiters)
    {
        this.Origin = origin;
        this.Position = i;
        this.CustomDelimiters = customDelimiters;
    }

    public StringCursor PullString(string fallback, out string result)
    {
        while (Current != char.MinValue && AtDelimiter)
            Position++;
        Anchor = Position;
        while (Current != char.MinValue && !AtDelimiter)
            Position++;
        if (Anchor == Position)
        {
            result = fallback;
            return this;
        }
        else
        {
            result = Origin.Substring(Anchor, Position - Anchor);
            return this;
        }
    }
}

public static class StringCursorExtensions
{
    public static StringCursor PullInt(this StringCursor sc, int fallback, out int result)
    {
        sc = sc.PullString(fallback.ToString(), out string byteString);
        if (!int.TryParse(byteString, out result))
            result = fallback;
        return sc;
    }
}