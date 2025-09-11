namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

public class DeobfuscatedFormDictionary : IReadOnlyDictionary<string, IEnumerable>, IDisposable
{
    private readonly string FormName;
    private readonly ICsrfFields Fields;
    private bool DisposedValue;
    private readonly IReadOnlyDictionary<string, IEnumerable> ValuesWithObfuscatedKeys;
    private readonly string[] ObfuscatedKeys;
    private readonly string[] DeobfuscatedKeys;
    private readonly Dictionary<string, string> DeobfuscationMap;
    public DeobfuscatedFormDictionary(IReadOnlyDictionary<string, IEnumerable> valuesWithObfuscatedFields, ICsrfFields fields, string formName)
    {
        this.FormName = formName;
        this.Fields = fields;
        this.ValuesWithObfuscatedKeys = valuesWithObfuscatedFields;
        this.DeobfuscatedKeys = Fields.GetSortedDeobfuscatedWhitelist(FormName);
        this.ObfuscatedKeys = Fields.GetSortedObfuscatedWhitelist(FormName);
        if (this.DeobfuscatedKeys.Length != this.ObfuscatedKeys.Length)
            throw new ArgumentException("Obfuscated and deobfuscated key lengths mismatch");
        this.DeobfuscationMap = ObfuscatedKeys.Zip(DeobfuscatedKeys, (ob, de) => (ob, de)).ToDictionary(x => x.ob, x => x.de);
    }
    public IEnumerable this[string key] => ValuesWithObfuscatedKeys[DeobfuscationMap[key]];
    public IEnumerable<string> Keys
    {
        get
        {
            using var enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current.Key;
            }
        }
    }
    public IEnumerable<IEnumerable> Values
    {
        get
        {
            using var enumerator = this.GetEnumerator();
            while (enumerator.MoveNext())
                yield return enumerator.Current.Value;
        }
    }
    public int Count => this.ObfuscatedKeys.Length;
    public bool ContainsKey(string key) => Keys.Any(x => x == key);
    public IEnumerator<KeyValuePair<string, IEnumerable>> GetEnumerator()
    {
        using var enumerator = ValuesWithObfuscatedKeys.GetEnumerator();
        SortedSet<string> unprocessedDeobfuscatedKeys = new(DeobfuscatedKeys);
        while (enumerator.MoveNext())
        {
            var obfuscatedKeyValue = enumerator.Current;
            if (DeobfuscationMap.TryGetValue(obfuscatedKeyValue.Key, out var trueName))
            {
                unprocessedDeobfuscatedKeys.Remove(trueName);
                yield return new KeyValuePair<string, IEnumerable>(trueName, obfuscatedKeyValue.Value);
            }
        }
        foreach (string unprocessedDeobfuscatedKey in unprocessedDeobfuscatedKeys)
        {
            yield return new KeyValuePair<string, IEnumerable>(unprocessedDeobfuscatedKey,
                Enumerable.Empty<object>());
        }
        yield break;
    }
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out IEnumerable value)
    {
        using var enumerator = this.GetEnumerator();
        while (enumerator.MoveNext())
        {
            if (enumerator.Current.Key == key)
            {
                value = enumerator.Current.Value;
                return true;
            }
        }
        value = Enumerable.Empty<object>();
        return false;
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    protected virtual void Dispose(bool disposing)
    {
        if (!DisposedValue)
        {
            if (disposing)
            {
                // TODO: dispose managed state (managed objects)
            }

            foreach (var item in Fields.GetSortedObfuscatedWhitelist(FormName))
                Fields.UnregisterAlias(FormName, item);

            DisposedValue = true;
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