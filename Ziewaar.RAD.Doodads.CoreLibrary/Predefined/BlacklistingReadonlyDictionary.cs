namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
public class BlacklistingReadonlyDictionary : IReadOnlyDictionary<string, object>
{
    private readonly IReadOnlyDictionary<string, object> _inner;
    private readonly HashSet<string> _blacklist;

    public BlacklistingReadonlyDictionary(
        IReadOnlyDictionary<string, object> inner,
        IEnumerable<string> blacklist)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _blacklist = new HashSet<string>(blacklist ?? Enumerable.Empty<string>());
    }

    public object this[string key]
    {
        get
        {
            if (_blacklist.Contains(key))
                throw new KeyNotFoundException();
            return _inner[key];
        }
    }

    public IEnumerable<string> Keys => _inner.Keys.Where(k => !_blacklist.Contains(k));

    public IEnumerable<object> Values => Keys.Select(k => _inner[k]);

    public int Count => _inner.Keys.Count(k => !_blacklist.Contains(k));

    public bool ContainsKey(string key)
    {
        return !_blacklist.Contains(key) && _inner.ContainsKey(key);
    }

    public bool TryGetValue(string key, out object value)
    {
        if (_blacklist.Contains(key))
        {
            value = null;
            return false;
        }
        return _inner.TryGetValue(key, out value);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        foreach (var kvp in _inner)
        {
            if (!_blacklist.Contains(kvp.Key))
                yield return kvp;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}