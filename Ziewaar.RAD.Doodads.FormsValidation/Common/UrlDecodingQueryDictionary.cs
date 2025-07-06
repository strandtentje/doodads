using System.Web;

namespace Ziewaar.RAD.Doodads.FormsValidation.Common;
#pragma warning disable 67
public class UrlDecodingQueryDictionary(
    string[] urlSanitizedWhitelist,
    string urlEncodedQuery,
    SortedList<string, string?> backingStore) :
    LazyDictionary(urlSanitizedWhitelist), IDecodingDictionary
{
    public static IDecodingDictionary CreateFor(string encodedData, string[] unsanitizedWhitelist) =>
        new UrlDecodingQueryDictionary(
            unsanitizedWhitelist.Select(HttpUtility.UrlEncode).OfType<string>().ToArray(),
            encodedData,
            new SortedList<string, string?>());
    protected override string? Sanitize(string unsanitizedKey) => HttpUtility.UrlEncode(unsanitizedKey);
    protected override string Desanitize(string sanitized) => HttpUtility.UrlDecode(sanitized);
    protected override string? FindValue(string sanitizedKey)
    {
        if (!urlSanitizedWhitelist.Contains(sanitizedKey))
            return null;
        if (backingStore.TryGetValue(sanitizedKey, out var cached))
            return cached;
        var keyPosition = urlEncodedQuery.StartsWith($"{sanitizedKey}=")
            ? 0
            : urlEncodedQuery.IndexOf($"&{sanitizedKey}=", StringComparison.OrdinalIgnoreCase);
        if (keyPosition < 0) return backingStore[sanitizedKey] = null;
        var equalsPosition = urlEncodedQuery.IndexOf('=', keyPosition + 1);
        if (equalsPosition < 0) return backingStore[sanitizedKey] = "";
        else equalsPosition += 1;
        var nextAmpersand = urlEncodedQuery.IndexOf('&', equalsPosition);
        var endingPosition = nextAmpersand < 0 ? urlEncodedQuery.Length : nextAmpersand;
        var encodedValue = urlEncodedQuery.Substring(equalsPosition, endingPosition - equalsPosition);
        return backingStore[sanitizedKey] = HttpUtility.UrlDecode(encodedValue);
    }
}
