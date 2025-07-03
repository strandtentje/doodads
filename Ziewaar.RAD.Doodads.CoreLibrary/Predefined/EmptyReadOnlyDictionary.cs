namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
#nullable enable

public sealed class EmptyReadOnlyDictionary : IReadOnlyDictionary<string, object>
{
    public static readonly EmptyReadOnlyDictionary Instance = new();
    private EmptyReadOnlyDictionary() { }
    public object this[string key] => throw new KeyNotFoundException();
    public IEnumerable<string> Keys => [];
    public IEnumerable<object> Values => [];
    public int Count => 0;
    public bool ContainsKey(string key) => false;
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield break;
    }
    public bool TryGetValue(string key, out object value)
    {
        value = null!;
        return false;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
