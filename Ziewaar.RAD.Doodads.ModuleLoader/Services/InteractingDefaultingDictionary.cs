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
            if (real.TryFindVariable<object>(key, out var result) && result != null)
                return result;
            return fallback[key];
        }
    }
    public SortedList<string, object> ToSortedList()
    {
        var result = new SortedList<string, object>();
        for (var currentInteraction = real; currentInteraction is not StopperInteraction; currentInteraction = currentInteraction.Stack)
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
    public bool TryGetValue(string key, out object value)
    {
        if (real.TryFindVariable(key, out object? realCandidate) && realCandidate != null)
        {
            value = realCandidate;
            return true;
        } else if (fallback.TryGetValue(key, out var fallbackCandidate) && fallbackCandidate != null)
        {
            value = fallbackCandidate;
            return true;
        }
        else
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return false;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => ToSortedList().GetEnumerator();
}