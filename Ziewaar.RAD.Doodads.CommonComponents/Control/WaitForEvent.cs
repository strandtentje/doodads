#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class WaitForEvent : IteratingService, IDisposable
{
    private bool _disposed;
    public void Dispose() => this._disposed = true;
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetClosest<ScopedEventInteraction>(out var ewh) || ewh == null)
            throw new Exception("No event was scoped.");

        while (!_disposed)
            if (ewh.Ewh.WaitOne(100))
                yield return repeater;
        yield break;
    }
}
