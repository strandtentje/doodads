using System.Collections;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class UrlEncodedQueryDictionary(
    string urlEncodedQuery,
    string[] urlSanitizedWhitelist,
    SortedList<string, string?> backingStore)
    : IReadOnlyDictionary<string, object>
{
    private string? FindValue(string urlSanitizedKey)
    {
        if (!urlSanitizedWhitelist.Contains(urlSanitizedKey))
            return null;
        if (backingStore.TryGetValue(urlSanitizedKey, out var cached))
            return cached;
        var keyPosition = urlEncodedQuery.StartsWith($"{urlSanitizedKey}=")
            ? 0
            : urlEncodedQuery.IndexOf($"&{urlSanitizedKey}=", StringComparison.OrdinalIgnoreCase);
        if (keyPosition < 0) return backingStore[urlSanitizedKey] = null;
        var equalsPosition = urlEncodedQuery.IndexOf('=', keyPosition + 1);
        if (equalsPosition < 0) return backingStore[urlSanitizedKey] = "";
        else equalsPosition += 1;
        var nextAmpersand = urlEncodedQuery.IndexOf('&', equalsPosition);
        var endingPosition = nextAmpersand < 0 ? urlEncodedQuery.Length : nextAmpersand;
        var encodedValue = urlEncodedQuery.Substring(equalsPosition, endingPosition - equalsPosition);
        return backingStore[urlSanitizedKey] = HttpUtility.UrlDecode(encodedValue);
    }
    public object this[string key] => FindValue(key) ?? throw new KeyNotFoundException();
    public IEnumerable<string> Keys => urlSanitizedWhitelist.Where(k => FindValue(k) != null);
    public IEnumerable<object> Values => Keys.Select(k => (object)this[k]);
    public int Count => Keys.Count();
    public bool ContainsKey(string key) => FindValue(key) != null;
    public bool TryGetValue(string key, [NotNullWhen(true)] out object? value) => (value = FindValue(key)) != null;
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Keys.Select(key => new KeyValuePair<string, object>(key, this[key])).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}