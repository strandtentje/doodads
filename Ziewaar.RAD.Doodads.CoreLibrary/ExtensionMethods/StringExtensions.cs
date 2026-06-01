namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class StringExtensions
{
    /// <summary>
    /// Splits the string by the specified string separator, with optional StringSplitOptions.
    /// </summary>
    public static string[] Split(this string str, string separator, StringSplitOptions options = StringSplitOptions.None)
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
}
