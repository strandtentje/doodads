namespace Ziewaar.RAD.Doodads.EnumerableStreaming;
#pragma warning disable 67
/// <summary>
/// Useful when it's certain the dictionary is going to be only
/// very incidentally queried, and the incidental performance tradeoff
/// is worth it.
/// </summary>
/// <param name="keyPrefix">Prefix to integrate with keys</param>
/// <param name="enumerable">Original enumerable</param>
public class LazilyPrefixedCaseInsensitiveDictionary(
    string keyPrefix,
    IEnumerable<KeyValuePair<string, object>> enumerable)
    : IReadOnlyDictionary<string, object>
{
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        using var enumerator = enumerable.GetEnumerator();
        while (enumerator.MoveNext())
            yield return new($"{keyPrefix}{enumerator.Current.Key}", enumerator.Current.Value);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => enumerable.Count();
    public bool ContainsKey(string key) =>
        key.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase)
        &&
        enumerable.Any(AppliesPrefixed(key));

    private Func<KeyValuePair<string, object>, bool> AppliesPrefixed(string soughtKey)
    {
        return candidateItem =>
        {
            if (!soughtKey.StartsWith(keyPrefix, StringComparison.OrdinalIgnoreCase))
                return false;
            string trueKeyToLookfor = soughtKey.Substring(keyPrefix.Length);
            return string.Equals(trueKeyToLookfor, candidateItem.Key,
                StringComparison.OrdinalIgnoreCase);
        };
    }

    public bool TryGetValue(string key, out object value)
    {
        var potentialMatches = enumerable.Where(AppliesPrefixed(key)).ToArray();
        if (potentialMatches.Length == 0)
        {
            value = new object();
            return false;
        }

        value = potentialMatches[0].Value;
        return true;
    }

    public object this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
    public IEnumerable<string> Keys => enumerable.Select(x => $"{keyPrefix}{x.Key}");
    public IEnumerable<object> Values => enumerable.Select(x => x.Value);
}