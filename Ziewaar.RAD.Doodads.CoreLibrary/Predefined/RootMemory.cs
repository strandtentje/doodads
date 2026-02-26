namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class RootMemory(IReadOnlyDictionary<string, object> memory) : IReadOnlyDictionary<string, object>
{
    private const string UTC_NOW = "utc-now";
    private const string LOCAL_NOW = "local-now";
    private static string GetLocalNow() => DateTime.Now.ToString("O");
    private static string GetUtcNow() => DateTime.UtcNow.ToString("O");

    public object this[string key] => this.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
    public IEnumerable<string> Keys
    {
        get
        {
            yield return UTC_NOW;
            yield return LOCAL_NOW;
            foreach (var item in memory.Keys)
                yield return item;
        }
    }
    public IEnumerable<object> Values
    {
        get
        {
            yield return GetUtcNow();
            yield return GetLocalNow();
            foreach (var item in memory.Values)
                yield return item;
        }
    }
    public int Count => memory.Count + 2;
    public bool ContainsKey(string key) =>
        key == UTC_NOW || key == LOCAL_NOW || memory.ContainsKey(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new KeyValuePair<string, object>(UTC_NOW, DateTime.UtcNow.ToString("O"));
        yield return new KeyValuePair<string, object>(LOCAL_NOW, DateTime.Now.ToString("O"));
        foreach (var item in memory)
            yield return item;
    }
    public bool TryGetValue(string key, out object value)
    {
        switch (key)
        {
            case UTC_NOW:
                value = GetUtcNow();
                return true;
            case LOCAL_NOW:
                value = GetLocalNow();
                return true;
            default:
                return memory.TryGetValue(key, out value);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}