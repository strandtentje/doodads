using System.Collections;
using Ziewaar.RAD.Doodads.RKOP.Text;

namespace Ziewaar.RAD.Doodads.RKOP.Constructor;
public class ServiceConstantsDescription : IParityParser, IReadOnlyDictionary<string, object>
{
    public List<ServiceConstantsMember> Members = new();
    public ParityParsingState UpdateFrom(ref CursorText text)
    {
        int oCounter = Members.Count;

        int memCounter = 0;
        ParityParsingState lastState = ParityParsingState.Void, finalState = ParityParsingState.Void;
        Token comma;
        do
        {
            if (Members.Count == memCounter)
                Members.Add(new ServiceConstantsMember());
            lastState = Members[memCounter].UpdateFrom(ref text);
            if (lastState > ParityParsingState.Void)
                memCounter++;
            finalState |= lastState;
            text = text.TakeToken(TokenDescription.ArgumentSeparator, out comma);
        } while (comma.IsValid);

        while (memCounter < Members.Count)
        {
            Members.RemoveAt(memCounter);
        }

        if (oCounter != Members.Count)
            finalState |= ParityParsingState.Changed;
        
        return finalState;
    }
    public void WriteTo(StreamWriter writer)
    {
        for (int i = 0; i < Members.Count; i++)
        {
            Members[i].WriteTo(writer);
            if (i < Members.Count - 1)
                writer.Write(", ");
        }
    }
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Members.ToDictionary(x => x.Key, x => x.Value.GetValue()).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    public int Count => Members.Count;
    public bool ContainsKey(string key) => Members.Any(x => x.Key == key);
    public bool TryGetValue(string key, out object value)
    {
        var found = Members.Where(x => x.Key == key).ToArray();
        value = found.FirstOrDefault()?.Value.GetValue();
        return found.Length == 1;
    }
    public object this[string key] => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException();
    public IEnumerable<string> Keys => Members.Select(x => x.Key);
    public IEnumerable<object> Values => Members.Select(x => x.Value.GetValue());
}
