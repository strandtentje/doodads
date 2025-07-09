using System.Net;
namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
public class FormDataDictionary : IReadOnlyDictionary<string, object>
{
    private readonly Dictionary<string, object> backingStore = new();

    public FormDataDictionary(string urlEncoded)
    {
        var pairs = urlEncoded.Split('&', StringSplitOptions.RemoveEmptyEntries);
        foreach (var pair in pairs)
        {
            var parts = pair.Split('=', 2);
            var key = WebUtility.UrlDecode(parts[0]);
            var value = parts.Length > 1 ? WebUtility.UrlDecode(parts[1]) : "";

            if (backingStore.TryGetValue(key, out var existing) && existing is List<string> list)
                list.Add(value);
            else
                backingStore[key] = new List<string> { value };
        }

        foreach (var kv in backingStore.ToList())
        {
            if (kv.Value is List<string> list)
                backingStore[kv.Key] = list.ToArray();
        }
    }

    public object this[string key] => backingStore[key];
    public IEnumerable<string> Keys => backingStore.Keys;
    public IEnumerable<object> Values => backingStore.Values;
    public int Count => backingStore.Count;
    public bool ContainsKey(string key) => backingStore.ContainsKey(key);
    public bool TryGetValue(string key, out object value) => backingStore.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => backingStore.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => backingStore.GetEnumerator();
}