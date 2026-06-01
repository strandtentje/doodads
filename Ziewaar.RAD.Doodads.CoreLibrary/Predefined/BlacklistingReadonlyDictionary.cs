namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class BlacklistingReadonlyDictionary(
    IReadOnlyDictionary<string, object> inner,
    IEnumerable<string> blacklist)
    : IReadOnlyDictionary<string, object>
{
    private readonly IReadOnlyDictionary<string, object> Inner =
        inner ?? throw new ArgumentNullException(nameof(inner));

    private readonly HashSet<string> Blacklist = new(blacklist);

    public object this[string key] =>
        Blacklist.Contains(key) ? throw new KeyNotFoundException() : Inner[key];

    public IEnumerable<string> Keys => Inner.Keys.Where(k => !Blacklist.Contains(k));

    public IEnumerable<object> Values => Keys.Select(k => Inner[k]);

    public int Count => Inner.Keys.Count(k => !Blacklist.Contains(k));

    public bool ContainsKey(string key) => !Blacklist.Contains(key) && Inner.ContainsKey(key);

    private static readonly object MissingKeyObject = new();
    
    public bool TryGetValue(string key, out object value)
    {
        if (Blacklist.Contains(key))
        {
            value = MissingKeyObject;
            return false;
        }

        return Inner.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (var kvp in Inner)
        {
            if (!Blacklist.Contains(kvp.Key))
                yield return kvp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}