namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public abstract class LazyDictionary(string[] sanitizedWhitelist)
    : IReadOnlyDictionary<string, object>
{
    protected abstract object? FindValue(string sanitizedKey);
    protected abstract string Sanitize(string unsanitized);
    protected abstract string Desanitize(string sanitized);
    public object this[string key] => FindValue(Sanitize(key)) ?? throw new KeyNotFoundException();
    public IEnumerable<string> Keys =>
        sanitizedWhitelist.Where(sanitizedKey => FindValue(sanitizedKey) != null).Select(Desanitize);
    public IEnumerable<object> Values => Keys.Select(sanitizedKey => (object)this[sanitizedKey]);
    public int Count => Keys.Count();
    public bool ContainsKey(string key) => FindValue(Sanitize(key)) != null;
    public bool TryGetValue(string key, [NotNullWhen(true)] out object? value) =>
        (value = FindValue(Sanitize(key))) != null;
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Keys.Select(sanitizedKey => new KeyValuePair<string, object>(Desanitize(sanitizedKey), this[sanitizedKey]))
            .GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}