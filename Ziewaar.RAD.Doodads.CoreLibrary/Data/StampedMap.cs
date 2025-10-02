namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
#nullable enable

public class StampedMap
{
    private readonly SortedList<string, object> BackingStore;
    private readonly SortedList<string, long> BackingLog;
    public SortedList<string, object> ToSortedList() =>
        new SortedList<string, object>(BackingStore);
    public void CopyNamedTo(IDictionary<string, object> target)
    {
        foreach (var item in BackingStore)
            target[item.Key] = item.Value;
    }
    public IReadOnlyDictionary<string, object> NamedItems => BackingStore;
    public IReadOnlyDictionary<string, long> ChangeLog => BackingLog;
    public object PrimaryConstant { get; private set; }
    public long PrimaryLog { get; private set; }

    public StampedMap(object primaryConstant) : base()
    {
        this.PrimaryConstant = primaryConstant;
        this.PrimaryLog = GlobalStopwatch.Instance.ElapsedTicks;
        this.BackingStore = new(0);
        this.BackingLog = new(0);
    }

    public StampedMap(object primaryConstant, IReadOnlyDictionary<string, object> origin)
    {
        var age = GlobalStopwatch.Instance.ElapsedTicks;
        this.PrimaryLog = age;
        this.PrimaryConstant = primaryConstant;
        this.BackingStore = new();
        this.BackingLog = new();
        foreach (var o in origin)
        {
            this.BackingLog.Add(o.Key, age);
            this.BackingStore.Add(o.Key, o.Value);
        }
    }

    public void SetPrimary(object primaryConstant)
    {
        if (this.PrimaryConstant != primaryConstant)
        {
            this.PrimaryConstant = primaryConstant;
            this.PrimaryLog = GlobalStopwatch.Instance.ElapsedTicks;
        }
    }

    public void SetValue(string key, object value)
    {
        if (!this.BackingStore.TryGetValue(key, out var preExisting) || preExisting != value)
        {
            BackingStore[key] = value;
            BackingLog[key] = GlobalStopwatch.Instance.ElapsedTicks;
        }
    }

    public void DeleteValue(string key)
    {
        BackingLog[key] = GlobalStopwatch.Instance.ElapsedTicks;
        BackingStore.Remove(key);
    }
}