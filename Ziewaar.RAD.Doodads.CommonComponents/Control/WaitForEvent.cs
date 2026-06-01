namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Block the scoped event")]
[Description("""
    Use with ScopeEvent, Block and SignalEvent to postpone doing work until a signal is received
    """)]
public class WaitForEvent : IteratingService, IDisposable
{
    protected override bool RunElse { get; }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if (!repeater.TryGetClosest<ScopedEventInteraction>(out var ewh) || ewh == null)
            throw new Exception("No event was scoped.");

        while (!IsDisposing)
            if (ewh.Ewh.WaitOne(100))
                yield return repeater;
        yield break;
    }
}
