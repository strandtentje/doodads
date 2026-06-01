namespace Ziewaar.RAD.Doodads.CommonComponents.ScopeMemory;

#pragma warning disable 67
[Category("Scheduling & Flow")]
[Title("Scope for debouncing")]
[Description("""
             Works with DebounceIn such that this only fires when the incoming
             Register value changes.
             """)]
public class DebounceOut : IService
{
    [EventOccasion("When a distinct value was detected in register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the same value was detected in register")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.TryGetCustom<DebounceContainer>(out var debouncer) && debouncer != null)
        {
            if (debouncer.LastValue?.ToString() != interaction.Register.ToString())
            {
                debouncer.LastValue = interaction.Register;
                OnThen?.Invoke(this, interaction);
            } else
            {
                OnElse?.Invoke(this, interaction);
            }
        } else
        {
            OnException?.Invoke(this, interaction.AppendRegister("debouncein required"));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}