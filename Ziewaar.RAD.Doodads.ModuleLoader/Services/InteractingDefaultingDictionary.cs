#pragma warning disable 67
#nullable enable

using System.Collections;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;

internal class InteractingDefaultingDictionary(IInteraction real, SortedList<string, object> fallback) : IReadOnlyDictionary<string, object>
{
    public object this[string key]
    {
        get
        {
            if (real.TryFindVariable<object>(key, out var result))
                return result;
            return fallback[key];
        }
    }
    public SortedList<string, object> ToSortedList()
    {
        var result = new SortedList<string, object>();
        for (var currentInteraction = real; currentInteraction != null && currentInteraction is not StopperInteraction; currentInteraction = currentInteraction.Stack)
        {
            foreach (var item in currentInteraction.Memory)
            {
                if (!result.ContainsKey(item.Key))
                {
                    result.Add(item.Key, item.Value);
                }
            }
        }
        return result;
    }
    public IEnumerable<string> Keys => ToSortedList().Keys;
    public IEnumerable<object> Values => ToSortedList().Values;
    public int Count => ToSortedList().Count;
    public bool ContainsKey(string key) => real.TryFindVariable<object>(key, out object? _) || fallback.ContainsKey(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => ToSortedList().GetEnumerator();
    public bool TryGetValue(string key, out object value) =>
        (real.TryFindVariable(key, out value) && value != null) || fallback.TryGetValue(key, out value);
    IEnumerator IEnumerable.GetEnumerator() => ToSortedList().GetEnumerator();
}