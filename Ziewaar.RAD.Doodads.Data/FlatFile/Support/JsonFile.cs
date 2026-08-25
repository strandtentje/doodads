using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.FlatFile.Support;

public class JsonFile
{
    private long LastReadTime = long.MinValue;
    private long CurrentVersionTime => File.GetLastWriteTimeUtc(BackingFile).Ticks;
    private Dictionary<string, Dictionary<string, object>>? BackingStore;
    private readonly string BackingFile;
    private readonly object ReloadLock = new object(), AccessLock = new object();
    public JsonFile(string filePath)
    {
        this.BackingFile = filePath;
        UseStore();
    }

    private Dictionary<string, Dictionary<string, object>> UseStore(Func<Dictionary<string, Dictionary<string, object>>, bool>? modifyingCallback = null)
    {
        lock (ReloadLock)
        {
            if (LastReadTime == CurrentVersionTime && BackingStore != null)
                return BackingStore;
            LastReadTime = CurrentVersionTime;
            lock (AccessLock)
            {
                if (!File.Exists(BackingFile))
                    File.WriteAllText(BackingFile, "{}");
                BackingStore = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(File.ReadAllText(BackingFile));
                BackingStore ??= new Dictionary<string, Dictionary<string, object>>();
            }
            if (modifyingCallback != null)
            {
                var hasChanged = modifyingCallback(BackingStore);
                if (hasChanged)
                {
                    var serialized = JsonConvert.SerializeObject(BackingStore, Formatting.Indented);
                    File.WriteAllText(BackingFile, serialized);
                    LastReadTime = CurrentVersionTime;
                }
            }
        }
        return BackingStore;
    }

    public IEnumerable<KeyValuePair<string, IReadOnlyDictionary<string, object>>> ListAll()
    {
        foreach (var item in UseStore())
            yield return new KeyValuePair<string, IReadOnlyDictionary<string, object>>(
                item.Key,
                item.Value);
    }

    public IEnumerable<KeyValuePair<string, IReadOnlyDictionary<string, object>>> ListAll(string member, Func<object, bool> test)
    {
        foreach (var item in UseStore())
        {
            if (item.Value.TryGetValue(member, out var toTest) && test(toTest))
                yield return new KeyValuePair<string, IReadOnlyDictionary<string, object>>(
                    item.Key,
                    item.Value);
        }
    }

    public IReadOnlyDictionary<string, object> ForKey(string key)
    {
        if (UseStore().TryGetValue(key, out var dict))
            return dict;
        else
            return EmptyReadOnlyDictionary.Instance;
    }

    public void Remove(string key)
    {
        UseStore(store => store.Remove(key));
    }

    public void Remove(string key, string member)
    {
        UseStore(store =>
        {
            if (!store.TryGetValue(key, out var toClear))
                return false;
            return toClear.Remove(member);
        });
    }

    public IReadOnlyDictionary<string, object> Set(string key, IEnumerable<(string member, object value)> values, bool overwrite = false)
    {
        var updatedStore = UseStore(store =>
        {
            bool hasChanged = false;
            if (!store.TryGetValue(key, out var target) || target == null || overwrite)
            {
                store[key] = target = new Dictionary<string, object>();
                hasChanged = true;
            }

            foreach (var item in values)
            {
                if (!target.TryGetValue(item.member, out var oldValue) || oldValue != item.value)
                {
                    target[item.member] = item.value;
                    hasChanged = true;
                }
            }
            return hasChanged;
        });
        return updatedStore[key];
    }
}
