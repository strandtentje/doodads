#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Block the scoped event")]
[Description("""
    Use with ScopeEvent, Block and WaitForEvent to signal branches elsewhere can begin their work
    """)]
public class SignalEvent : IService
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
        ewh.Ewh.Set();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
