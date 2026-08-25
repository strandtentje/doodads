using System.Globalization;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class RootMemory(IReadOnlyDictionary<string, object> memory) : IReadOnlyDictionary<string, object>
{
    private const string UTC_NOW = "utc-now";
    private const string LOCAL_NOW = "local-now";
    private const string LOCAL_NOW_FILESAFE = "local-now-filesafe";
    private const string FRESH_GUID = "fresh-guid";
    private static string GetLocalNow() => DateTime.Now.ToString("O");
    private static string GetUtcNow() => DateTime.UtcNow.ToString("O");
    private static string GetFileNow() => "'" + DateTime.Now.ToString("yy-MM-dd,HH:mm;ss", CultureInfo.InvariantCulture).Replace(':','h');
    private static string GetFreshGuid() => Guid.NewGuid().ToString();
    public object this[string key] => this.TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
    public IEnumerable<string> Keys
    {
        get
        {
            yield return UTC_NOW;
            yield return LOCAL_NOW;
            yield return LOCAL_NOW_FILESAFE;
            yield return FRESH_GUID;
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
            yield return GetFileNow();
            yield return GetFreshGuid();
            foreach (var item in memory.Values)
                yield return item;
        }
    }
    public int Count => memory.Count + 3;
    public bool ContainsKey(string key) =>
        key == UTC_NOW || key == LOCAL_NOW || key == LOCAL_NOW_FILESAFE || key == FRESH_GUID || memory.ContainsKey(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new KeyValuePair<string, object>(UTC_NOW, DateTime.UtcNow.ToString("O"));
        yield return new KeyValuePair<string, object>(LOCAL_NOW, DateTime.Now.ToString("O"));
        yield return new KeyValuePair<string, object>(LOCAL_NOW_FILESAFE, GetFileNow());
        yield return new KeyValuePair<string, object>(FRESH_GUID, GetFreshGuid());
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
            case LOCAL_NOW_FILESAFE:
                value = GetFileNow();
                return true;
            case FRESH_GUID:
                value = GetFreshGuid();
                return true;
            default:
                return memory.TryGetValue(key, out value);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}