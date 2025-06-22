#nullable enable
#pragma warning disable 67

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;

namespace Ziewaar.RAD.Doodads.Data;

public class DictionaryAroundReader(IDataReader reader) : IReadOnlyDictionary<string, object>
{
    public object this[string key] => reader.GetValue(Array.IndexOf(internalKeyArray, key)).ConvertNumericToDecimal();
    public readonly string[] internalKeyArray = Enumerable.Range(0, reader.FieldCount).Select(x => reader.GetName(x)).ToArray();
    public IEnumerable<string> Keys => internalKeyArray;
    public IEnumerable<object> Values => Enumerable.Range(0, reader.FieldCount).Select(x => reader.GetValue(x).ConvertNumericToDecimal());
    public int Count => internalKeyArray.Length;
    public bool ContainsKey(string key) => Array.IndexOf(internalKeyArray, key) >= 0;

    private IEnumerator<KeyValuePair<string, object>> GenerateEnumerator() => 
        Enumerable.
        Range(0, reader.FieldCount).
        Select(
            x => new KeyValuePair<string, object>(
                internalKeyArray[x],
                reader.GetValue(x).ConvertNumericToDecimal())).
        GetEnumerator();
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => GenerateEnumerator();
    public bool TryGetValue(string key, out object value)
    {
        var keypos = Array.IndexOf(internalKeyArray, key);
        if (keypos < 0)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return false;
        }
        else
        {
            value = reader.GetValue(keypos).ConvertNumericToDecimal();
            return true;
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GenerateEnumerator();
}
