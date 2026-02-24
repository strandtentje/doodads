using System.Reflection;
using System.Runtime.CompilerServices;

namespace Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

public static class StringExtensions
{
    public static int InTwoBy(this string str, char sep, out string left, out string right)
    {
        var parts = str.Split([sep], 2, StringSplitOptions.RemoveEmptyEntries);
        left = parts.ElementAtOrDefault(0)?.Trim() ?? "";
        right = parts.ElementAtOrDefault(1)?.Trim() ?? "";
        return parts.Length;
    }
    public static void InThreeBy(this string str, char sep, out string left, out string middle, out string right)
    {
        var parts = str.Split([sep], 3, StringSplitOptions.RemoveEmptyEntries);
        left = parts.ElementAtOrDefault(0)?.Trim() ?? "";
        middle = parts.ElementAtOrDefault(1)?.Trim() ?? "";
        right = parts.ElementAtOrDefault(2)?.Trim() ?? "";

    }
    public static void InFourBy(this string str, char sep, out string left, out string lm, out string rm, out string right)
    {
        var parts = str.Split([sep], 4, StringSplitOptions.RemoveEmptyEntries);
        left = parts.ElementAtOrDefault(0)?.Trim() ?? "";
        lm = parts.ElementAtOrDefault(1)?.Trim() ?? "";
        rm = parts.ElementAtOrDefault(2)?.Trim() ?? "";
        right = parts.ElementAtOrDefault(3)?.Trim() ?? "";
    }

    public static bool StartsWith(this string str, string startsWith, out string remainder)
    {
        if (str.StartsWith(startsWith, StringComparison.OrdinalIgnoreCase))
        {
            remainder = str.Substring(startsWith.Length);
            return true;
        }
        else
        {
            remainder = "";
            return false;
        }
    }
    public static IReadOnlyDictionary<string, string> ToDictionary(this string[] pairs) => pairs.
        Select(x => x.InTwoBy('=', out string key, out string value) > 0 ? (key, value) : (key: "", value: "")).
        DistinctByAny(x => x.key).ToDictionary(x => x.key, x => x.value, StringComparer.OrdinalIgnoreCase);
    public static IEnumerable<T> DistinctByAny<T>(
        this IEnumerable<T> items, Func<T, IComparable> discriminator, 
        SortedSet<IComparable> existing = null) => 
        items.Where(x => (existing ??= []).Add(discriminator(x)));
    public static TOutput GetParsedOrDefault<TOutput>(
        this (IReadOnlyDictionary<string, string> dict, string key) lookup,
        Func<string, TOutput> parser,
        TOutput defaultValue = default)
    {
        try
        {
            return lookup.dict.TryGetValue(lookup.key, out string serialValue) ? parser(serialValue) : defaultValue;
        } catch(Exception)
        {
            // whatever
            return defaultValue;
        }
    }
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

    public static string Alphanumerize(this string text) => new string(
                text.ToLower().Select(c => char.IsLetterOrDigit(c) || c == '_' ? c : '_').ToArray());

}
