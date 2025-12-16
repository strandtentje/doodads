#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;


[Category("Scheduling & Flow")]
[Title("Block the scoped event")]
[Description("""
    Use with ScopeEvent, SignalEvent and WaitForEvent to run or prevent running 
    of branches elsewhere.
    """)]
public class BlockEvent : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetClosest<ScopedEventInteraction>(out var ewh) || ewh == null)
            throw new Exception("No event was scoped.");
        ewh.Ewh.Reset();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
