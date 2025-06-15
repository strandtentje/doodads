namespace Ziewaar.RAD.Doodads.ModuleLoader;
#nullable enable

public class DiagnosticConstant(string key, long timestamp, object value)
{
    public string Key => key;
    public long Timestamp => timestamp;
    public object Value => value;
    public static DiagnosticConstant Create(KeyValuePair<string, object> value, KeyValuePair<string, long> timestamp) => 
        new DiagnosticConstant(key: value.Key, timestamp: timestamp.Value, value: value.Value);
}
