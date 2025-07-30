#nullable enable

using Serilog;

namespace Ziewaar.RAD.Doodads.CoreLibrary;

public class SwitchingDictionary(
    string[] members,
    Func<string, object> valueSource) : IReadOnlyDictionary<string, object>
{
    public object this[string key] => key switch
    {
        null => throw new ArgumentNullException(),
        _ => valueSource(key)
    };

    public IEnumerable<string> Keys => members;
    public IEnumerable<object> Values => members.Select(x => valueSource(x));
    public int Count { get; } = members.Length;
    public bool ContainsKey(string key) => members.Contains(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        members.ToDictionary(x => x, valueSource).GetEnumerator();
    public bool TryGetValue(string key, out object value)
    {
        value = null;
        if (key == null) return false;
        if (!members.Contains(key)) return false;
        value = valueSource(key);
        return true;
    }
    IEnumerator IEnumerable.GetEnumerator() =>
        members.ToDictionary(x => x, valueSource).GetEnumerator();
}

public static class GlobalLog
{
    public static ILogger? Instance = null;
}