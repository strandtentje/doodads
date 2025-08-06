using System.Net;

namespace Ziewaar.RAD.Doodads.FormsValidation.HTML;
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

public class DeobfuscatedFormDictionary : IReadOnlyDictionary<string, IEnumerable>, IDisposable
{
    private readonly string FormName;
    private ICsrfFields Fields;
    private bool disposedValue;
    private readonly IReadOnlyDictionary<string, IEnumerable> ObfuscatedStore;

    public DeobfuscatedFormDictionary(IReadOnlyDictionary<string, IEnumerable> obfuscatedFields, ICsrfFields fields, string formName)
    {
        this.FormName = formName;
        this.Fields = fields;
        this.ObfuscatedStore = obfuscatedFields;
        this.Keys = Fields.GetDeobfuscatedWhitelist(FormName);
        this.Values = Fields.GetObfuscatedWhitelist(FormName).Select(x => obfuscatedFields.TryGetValue(x, out var val) ? val : Enumerable.Empty<object>());
        this.Count = Keys.Count();
    }

    public IEnumerable this[string key]
    {
        get
        {
            if (Fields.TryObfuscating(FormName, key, out var obfuscatedName) &&
                !string.IsNullOrWhiteSpace(obfuscatedName))
            {
                if (ObfuscatedStore.TryGetValue(obfuscatedName, out var item))
                {
                    return item;
                }
                else
                {
                    return Enumerable.Empty<object>();
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }
    }
    public IEnumerable<string> Keys { get; }
    public IEnumerable<IEnumerable> Values { get; }
    public int Count { get; }
    public bool ContainsKey(string key) => Keys.Any(x => x == key);
    public IEnumerator<KeyValuePair<string, IEnumerable>> GetEnumerator() => Keys.ToDictionary(x => x, x => this[x]).GetEnumerator();
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IEnumerable value)
    {
        if (Fields.TryObfuscating(FormName, key, out var obfuscatedName) &&
            !string.IsNullOrWhiteSpace(obfuscatedName) &&
            ObfuscatedStore.TryGetValue(obfuscatedName, out var item))
        {
            value = item;
            return true;
        }
        value = null;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            foreach (var item in Fields.GetObfuscatedWhitelist(FormName))
                Fields.UnregisterAlias(FormName, item);

            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~DeobfuscatedFormDictionary()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
