namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

#pragma warning disable 67
[Category("Scheduling & Flow")]
[Title("Scope for debouncing")]
[Description("""
             For invocations that might fire multiple times with the same register value;
             a bit like calling .Distinct()

             Use this for opening a Debouncing scope in which DebounceOut can work to
             check register changes.
             """)]
public class DebounceIn : IService
{
    [EventOccasion("Debounce scope here")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, interaction.AppendCustom(new DebounceContainer()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}