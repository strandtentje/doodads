#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Data;
public interface IUpdatingValue
{
    bool WriteSeen { get; }
    void UpdateInto(StampedMap map, object value);
    bool IsUpdateRequiredFrom(StampedMap map, out object? value);
}