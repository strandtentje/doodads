#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Block the scoped event")]
[Description("""
    Brings a named event in scope. Use with BlockEvent, SignalEvent and WaitForEvent.
    """)]
public class ScopeEvent : IService
{
    [PrimarySetting("Reset style. Auto for letting one branch through, Manual for multiple until reset.")]
    private static readonly UpdatingPrimaryValue ResetModeConstant = new();
    public static readonly SingletonResourceRepository<string, EventWaitHandle> EwhRepo =
        SingletonResourceRepository<string, EventWaitHandle>.Get();    
    private EventResetMode CurrentResetMode = EventResetMode.AutoReset;
    [EventOccasion("Sink name of event here.")]
    public event CallForInteraction? Name;
    [EventOccasion("Event is in scope here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the name for the event wasn't complete or empty")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ResetModeConstant).IsRereadRequired(out string? resetModeCandidate) &&
            resetModeCandidate != null && Enum.TryParse<EventResetMode>(resetModeCandidate, out var resetMode))
            this.CurrentResetMode = resetMode;

        var tsi = new TextSinkingInteraction(interaction);
        Name?.Invoke(this, tsi);
        var ewhName = tsi.ReadAllText();

        if (string.IsNullOrWhiteSpace(ewhName))
            ewhName = interaction.Register.ToString();
        if (ewhName == null || string.IsNullOrWhiteSpace(ewhName))
        {
            OnException?.Invoke(this, interaction.AppendRegister("no name for event given"));
            return;
        }

        var ewh = EwhRepo.Take(ewhName, x => new EventWaitHandle(false, CurrentResetMode));        
        try
        {
            OnThen?.Invoke(this, new ScopedEventInteraction(interaction, ewh.Instance));
        }
        finally
        {
            EwhRepo.Return(ewhName, ewh.Guid);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
