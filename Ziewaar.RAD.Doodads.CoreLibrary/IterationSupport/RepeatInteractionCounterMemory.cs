#nullable enable


using System.Globalization;
using TtLog;

namespace Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

internal class RepeatInteractionCounterMemory(string repeatName, IInteraction above) : IReadOnlyDictionary<string, object>
{
    private int Counter = -1;
    public int GetCounterValue() => Counter;
    public object this[string key] { get => TryGetValue(key, out var val) ? val : throw new KeyNotFoundException(); }
    private string PlainNumberKey => field ??= $"Number of {repeatName}";
    private string PlainIndexKey => field ??= $"Index of {repeatName}";
    public IEnumerable<string> Keys => [PlainNumberKey, PlainIndexKey];
    public IEnumerable<object> Values => [Counter + 1, Counter];
    public int Count { get; } = 2;
    public bool ContainsKey(string key) => TryGetValue(key, out var _);
    private static readonly Log MissingVar = Log.Warn("Missing variable in special repeat query");
    public bool TryGetValue(string key, out object value)
    {
        value = Counter;
        string? remainderString;
        decimal workingValue;
        if (key.StartsWith(PlainNumberKey))
        {
            remainderString = key.Substring(PlainNumberKey.Length);
            workingValue = Counter + 1;
        } else if (key.StartsWith(PlainIndexKey))
        {
            remainderString = key.Substring(PlainIndexKey.Length);
            workingValue = Counter;
        } else
        {
            return false;
        }
        
        for (var puller = remainderString.PullEnum(CounterOperator.Stop, out var rator).PullString(string.Empty, out string randStr);
            rator != CounterOperator.Stop && !string.IsNullOrWhiteSpace(randStr);
            puller = puller.PullEnum(CounterOperator.Stop, out rator).PullString(string.Empty, out randStr))
        {
            decimal rand;
            if (randStr.StartsWith("{") && randStr.EndsWith("}"))
            {
                var randKey = randStr.Trim('{', '}');
                if (above.TryFindVariable(randKey, out object? rv) && rv != null)
                {
                    rand = Convert.ToDecimal(rv, CultureInfo.InvariantCulture);
                } else
                {
                    Log.Post(MissingVar);
                    rand = 0;
                    if (rator == CounterOperator.Times || rator == CounterOperator.Divide)
                        rand = 1;
                }
            } else
            {
                rand = Convert.ToDecimal(randStr, CultureInfo.InvariantCulture);
            }
            switch (rator)
            {
                case CounterOperator.Minus:
                    workingValue -= rand;
                    break;
                case CounterOperator.Plus:
                    workingValue += rand;
                    break;
                case CounterOperator.Times:
                    workingValue *= rand;
                    break;
                case CounterOperator.Divide:
                    workingValue /= rand;
                    break;
                case CounterOperator.Mod:
                    workingValue %= rand;
                    break;
                default:
                    break;
            }
        }
        value = workingValue;
        return true;
    }

    internal void Increment() => Counter++;
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
        yield return new KeyValuePair<string, object>(PlainNumberKey, Counter);
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
