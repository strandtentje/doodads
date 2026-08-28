using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
#nullable enable

public interface ITextCursor
{
    string? File { get; }
    int Line { get; }
    int Column { get; }
}

public class StampedMap
{
    private readonly ConcurrentDictionary<string, object> BackingStore;
    private readonly ConcurrentDictionary<string, long> BackingLog;
    public readonly ITextCursor? PositionInFile;
    public string? DefiningFile => PositionInFile?.File;
    public int Line => PositionInFile?.Line ?? 0;
    public int Column => PositionInFile?.Column ?? 0;
    private readonly object LockObject = new();

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
        this.BackingStore = new();
        this.BackingLog = new();
    }

    public StampedMap(object primaryConstant, IReadOnlyDictionary<string, object> origin, ITextCursor? definingFile = null)
    {
        var age = GlobalStopwatch.Instance.ElapsedTicks;
        this.PrimaryLog = age;
        this.PrimaryConstant = primaryConstant;
        this.BackingStore = new();
        this.BackingLog = new();
        this.PositionInFile = definingFile;
        foreach (var o in origin)
        {
            lock (LockObject)
            {
                if (!(this.BackingLog.TryAdd(o.Key, age) && this.BackingStore.TryAdd(o.Key, o.Value)))
                {
                    GlobalLog.Instance?.Warning("failed to add {key} to settings map", o.Key);
                }
            }
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
        lock (LockObject)
        {
            if (!this.BackingStore.TryGetValue(key, out var preExisting) || preExisting != value)
            {
                BackingStore[key] = value;
            }
            if (!this.BackingLog.ContainsKey(key))
            {
                BackingLog[key] = GlobalStopwatch.Instance.ElapsedTicks;
            }
        }
    }

    public void DeleteValue(string key)
    {
        lock (LockObject)
        {
            BackingLog[key] = GlobalStopwatch.Instance.ElapsedTicks;
            if (!BackingStore.TryRemove(key, out var _))
            {
                GlobalLog.Instance?.Warning("failed to remove {key} from settings map", key);
            }
        }
    }
}