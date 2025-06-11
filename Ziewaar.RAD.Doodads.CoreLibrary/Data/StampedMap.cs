namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
#nullable enable

public class StampedMap
{
    private readonly SortedList<string, object> BackingStore;
    private readonly SortedList<string, long> BackingLog;
    public SortedList<string, object> ToSortedList() => new SortedList<string, object>(BackingStore);
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

public interface IUpdatingValue
{
    bool WriteSeen { get; }
    void UpdateInto(StampedMap map, object value);
    bool IsUpdateRequiredFrom(StampedMap map, out object? value);
}

public static class UpdatingValueExtensions
{
    public static bool IsRereadRequired<TResult>(
        this (StampedMap map, IUpdatingValue value) selection, out TResult? value) =>
        selection.IsRereadRequired(() => default, out value);
    public static bool IsRereadRequired<TResult>(
        this (StampedMap map, IUpdatingValue value) selection,
        Func<TResult?> sourceDefault,
        out TResult? value)
    {
        if (selection.value.IsUpdateRequiredFrom(selection.map, out var candidate))
        {
            if (candidate is TResult resultCandidate)
            {
                value = resultCandidate;
            }
            else if (sourceDefault() is { } defaultCandidate)
            {
                selection.value.UpdateInto(selection.map, defaultCandidate);
                value = defaultCandidate;
            }
            else
            {
                value = default(TResult);
            }
            return true;
        }
        else
        {
            if (candidate is TResult resultCandidate)
            {
                value = resultCandidate;
            }
            else if (sourceDefault() is { } defaultCandidate)
            {
                selection.value.UpdateInto(selection.map, defaultCandidate);
                value = defaultCandidate;
            }
            else
            {
                value = default(TResult);
            }
            return false;
        }
    }
}

public class UpdatingPrimaryValue : IUpdatingValue
{
    public long LastWrite { get; private set; } = long.MaxValue;
    public long LastRead { get; private set; } = long.MinValue;
    public bool WriteSeen => LastWrite <= LastRead;

    public void UpdateInto(StampedMap map, object value)
    {
        map.SetPrimary(value);
        LastWrite = map.PrimaryLog;
    }

    public bool IsUpdateRequiredFrom(StampedMap map, out object? value)
    {
        LastWrite = map.PrimaryLog;
        value = map.PrimaryConstant;
        if (WriteSeen)
            return false;
        LastRead = GlobalStopwatch.Instance.ElapsedTicks;
        return true;
    }
}

public class UpdatingKeyValue(string key) : IUpdatingValue
{
    public string Key => key;
    public long LastWrite { get; private set; } = long.MaxValue;
    public long LastRead { get; private set; } = long.MinValue;
    public bool WriteSeen => LastWrite <= LastRead;

    public void UpdateInto(StampedMap map, object value)
    {
        map.SetValue(this.Key, value);
        LastWrite = map.ChangeLog[this.Key];
    }

    public bool IsUpdateRequiredFrom(StampedMap map, out object? value)
    {
        if (map.ChangeLog.TryGetValue(this.Key, out var age))
            LastWrite = age;
        map.NamedItems.TryGetValue(this.Key, out value);
        if (WriteSeen)
            return false;
        LastRead = GlobalStopwatch.Instance.ElapsedTicks;
        return true;
    }
}