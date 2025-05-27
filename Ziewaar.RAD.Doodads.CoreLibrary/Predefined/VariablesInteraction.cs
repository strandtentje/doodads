using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VariablesInteraction(IInteraction parent, SortedList<string, object> variables) : IInteraction
{
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables => variables;
    public static VariablesInteraction ForError(
        IInteraction parent, string message) => new VariablesInteraction(
            parent, new SortedList<string, object> { { "error", message } });

    public static VariablesInteractionBuilder CreateBuilder(IInteraction parent) => new VariablesInteractionBuilder(parent);
}

public class StreamToVariableInteraction(IInteraction parent, string variableName, ISourcingInteraction<Stream> relevant) : IInteraction
{
    public IInteraction Parent => parent;

    public IReadOnlyDictionary<string, object> Variables { get; } = new LazySingleKeyDictionary(
        variableName,
        () =>
        {
            using var reader = new StreamReader(relevant.TaggedData.Data);
            return reader.ReadToEnd();
        });
}

public class LazySingleKeyDictionary(string key, Func<string> valueFactory)
    : IReadOnlyDictionary<string, object>
{
    public object this[string k] =>
        k == key ? valueFactory() : throw new KeyNotFoundException();

    public IEnumerable<string> Keys => [key];

    public IEnumerable<object> Values => [valueFactory()];

    public int Count => 1;

    public bool ContainsKey(string k) => k == key;

    public bool TryGetValue(string k, out object value)
    {
        value = k == key ? valueFactory() : string.Empty;
        return k == key;
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new(key, valueFactory());
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}