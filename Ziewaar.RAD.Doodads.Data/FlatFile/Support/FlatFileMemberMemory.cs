using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Ziewaar.RAD.Doodads.Data.FlatFile.Support;

public class FlatFileMemberMemory(IReadOnlyDictionary<string, object> defaults, IReadOnlyDictionary<string, object> assigns) : IReadOnlyDictionary<string, object>
{
    public object this[string key] => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();
    public IEnumerable<string> Keys => defaults.Keys;
    public IEnumerable<object> Values => defaults.Keys.Select(x => this[x]);
    public int Count { get; } = defaults.Count;
    public bool ContainsKey(string key) => defaults.ContainsKey(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Keys.Select(x => new KeyValuePair<string, object>(x, this[x])).GetEnumerator();
    public bool TryGetValue(string key, out object value)
    {
        value = "";
        if (!defaults.TryGetValue(key, out var dflt)) return false;
        value = assigns.TryGetValue(key, out var cand) ? cand : dflt;
        return true;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}