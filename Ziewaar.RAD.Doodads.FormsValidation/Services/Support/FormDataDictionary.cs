namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support;
public class FormDataDictionary : IReadOnlyDictionary<string, IEnumerable>
{
    private readonly Dictionary<string, IEnumerable> backingStore = new();
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
    public IEnumerable this[string key] => backingStore[key];
    public IEnumerable<string> Keys => backingStore.Keys;
    public IEnumerable<IEnumerable> Values => backingStore.Values;
    public int Count => backingStore.Count;
    public bool ContainsKey(string key) => backingStore.ContainsKey(key);
    public bool TryGetValue(string key, out IEnumerable value) => backingStore.TryGetValue(key, out value);
    public IEnumerator<KeyValuePair<string, IEnumerable>> GetEnumerator() => backingStore.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => backingStore.GetEnumerator();
}