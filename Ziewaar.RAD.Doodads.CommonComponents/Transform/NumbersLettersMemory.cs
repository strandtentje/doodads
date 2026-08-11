using System.Collections;
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Transform;

internal class NumbersLettersMemory(
    string resultKeyPrefix, StringBuilder numberPart, StringBuilder letterPart,
    string defaultNumbers, string defaultLetters) : IReadOnlyDictionary<string, object>
{
    public readonly string NumberKey = $"{resultKeyPrefix}numbers";
    public readonly string LetterKey = $"{resultKeyPrefix}letters";
    public string NumberPart => field ??= numberPart.ToString();
    public string LetterPart => field ??= letterPart.ToString();

    public bool TryGetValue(string key, out object value)
    {
        if (key == NumberKey)
        {
            if (string.IsNullOrWhiteSpace(NumberPart))
                value = defaultNumbers;
            else
                value = NumberPart;
            return true;
        }
        else if (key == LetterKey)
        {
            if (string.IsNullOrWhiteSpace(LetterPart))
                value = defaultLetters;
            else
                value = LetterPart;
            return true;
        }
        else
        {
            value = string.Empty;
            return false;
        }
    }
    public object this[string key] => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException();
    public IEnumerable<string> Keys => [NumberKey, LetterKey];
    public IEnumerable<object> Values => [NumberPart, LetterPart];
    public int Count => 2;
    public bool ContainsKey(string key) => Keys.Contains(key);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new KeyValuePair<string, object>(NumberKey, NumberPart);
        yield return new KeyValuePair<string, object>(LetterKey, LetterPart);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}