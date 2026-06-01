#pragma warning disable 67
#nullable enable

namespace Ziewaar.RAD.Doodads.CoreLibrary;

public class InteractionMirroringDictionary(IInteraction backingInteraction) : IReadOnlyDictionary<string, object>
{
    public object this[string key]
    {
        get
        {
            if (backingInteraction.TryFindVariable<object>(key,
                    out var result) && result != null)
                return result;
            else
                throw new KeyNotFoundException();
        }
    }
    public SortedList<string, object> ToSortedList()
    {
        var result = new SortedList<string, object>();
        for (var currentInteraction = backingInteraction; 
             currentInteraction is not StopperInteraction; 
             currentInteraction = currentInteraction.Stack)
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
    public bool ContainsKey(string key) => backingInteraction.TryFindVariable<object>(key, out object? _);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => ToSortedList().GetEnumerator();
    private static readonly object NoKeyObject = new();
    public bool TryGetValue(string key, out object value)
    {
        if (backingInteraction.TryFindVariable(key, out object? realCandidate) && realCandidate != null)
        {
            value = realCandidate;
            return true;
        } 
        else
        {
            value = NoKeyObject;
            return false;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => ToSortedList().GetEnumerator();
}