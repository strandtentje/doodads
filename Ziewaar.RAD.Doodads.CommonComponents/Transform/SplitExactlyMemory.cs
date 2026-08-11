using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

public class SplitExactlyMemory(string[] names, Dictionary<string, object> replacements, string[] splitValues) : IReadOnlyDictionary<string, object>
{
    public string FinalDefault => replacements.TryGetValue("", out var dc) && dc.ToString() is string dcs ? dcs : "";

    public bool TryGetValue(string key, out object value)
    {
        var position = names.IndexOf(key);
        if (position < 0)
        {
            value = string.Empty;
            return false;
        }
        else
        {
            HashSet<string> antiCircularSet = new HashSet<string>();
            antiCircularSet.Add(key);
            while(position >= splitValues.Length)
            {
                if (replacements.TryGetValue(key, out var newKey) && newKey?.ToString() is string newKeyString)
                {
                    if (!antiCircularSet.Add(newKeyString))
                    {
                        GlobalLog.Instance?.Warning("Circular defaults in SplitExactly; {names}", string.Join(",", antiCircularSet));
                        value = FinalDefault;
                        return true;
                    }
                    key = newKeyString;
                    position = names.IndexOf(key);
                } else
                {
                    value = FinalDefault;
                    return true;
                }
            }
            if (position < 0)
            {
                value = FinalDefault;
                return true;
            }
            value = splitValues[position];
            return true;
        }
    }

    public object this[string key] => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();

    public IEnumerable<string> Keys => names;
    public IEnumerable<object> Values => names.Select(x => this[x]);
    public int Count => names.Length;
    public bool ContainsKey(string key) => names.Contains(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => names.ToDictionary(x => x, x => this[x]).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}