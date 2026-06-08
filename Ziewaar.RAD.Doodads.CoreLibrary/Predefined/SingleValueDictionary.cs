#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class SingleValueDictionary : IReadOnlyDictionary<string, object>
{
    public string SingleKey { get; set; } = "unused";
    public object SingleValue { get; set; } = "unset";
    public object this[string key] => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();
    public IEnumerable<string> Keys => [SingleKey];
    public IEnumerable<object> Values => [SingleValue];
    public int Count { get; } = 1;
    public bool ContainsKey(string key) => key == SingleKey;
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new KeyValuePair<string, object>(SingleKey, SingleValue);
    }
    public bool TryGetValue(string key, out object value)
    {
        value = SingleValue;
        return key == SingleKey;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
