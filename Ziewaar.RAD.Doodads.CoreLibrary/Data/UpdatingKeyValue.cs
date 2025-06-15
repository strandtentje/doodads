#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
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