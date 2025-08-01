namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
public class FallbackReadOnlyDictionary(IReadOnlyDictionary<string, object> primary, IReadOnlyDictionary<string, object> secondary)
    : IReadOnlyDictionary<string, object>
{
    public object this[string key]
        => primary.TryGetValue(key, out var value)
            ? value
            : secondary.TryGetValue(key, out value)
                ? value
                : throw new KeyNotFoundException($"Key '{key}' was not found in either dictionary.");

    public IEnumerable<string> Keys
        => new HashSet<string>(primary.Keys.Concat(secondary.Keys));

    public IEnumerable<object> Values
        => Keys.Select(k => this[k]);

    public int Count
        => Keys.Count();

    public bool ContainsKey(string key)
        => primary.ContainsKey(key) || secondary.ContainsKey(key);

    public bool TryGetValue(string key, out object value)
    {
        if (primary.TryGetValue(key, out value)) return true;
        if (secondary.TryGetValue(key, out value)) return true;
        value = default!;
        return false;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        var seen = new HashSet<string>();
        foreach (var kv in primary)
        {
            seen.Add(kv.Key);
            yield return kv;
        }

        foreach (var kv in secondary)
        {
            if (seen.Add(kv.Key)) // Avoid duplicates
                yield return kv;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}