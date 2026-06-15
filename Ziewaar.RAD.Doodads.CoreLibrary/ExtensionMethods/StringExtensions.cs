using System.Globalization;

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
    
    
    public static string RemoveDiacritics(this string text) 
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    public static IEnumerable<string> SplitAtNonAlpha(this string splitText)
    {
        StringBuilder op = new();

        foreach (var t in splitText)
        {
            if (char.IsLetterOrDigit(t))
                op.Append(t);
            else if (op.Length > 0)
            {
                yield return op.ToString();
                op.Clear();
            }
        }

        if (op.Length <= 0) yield break;
        
        yield return op.ToString();
        op.Clear();
    }
}
