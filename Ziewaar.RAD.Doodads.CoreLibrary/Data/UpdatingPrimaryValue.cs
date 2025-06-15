#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
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