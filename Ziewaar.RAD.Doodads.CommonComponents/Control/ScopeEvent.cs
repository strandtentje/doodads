#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class ScopeEvent : IService
{
    private static readonly UpdatingPrimaryValue ResetModeConstant = new();
    public static readonly SingletonResourceRepository<string, EventWaitHandle> EwhRepo =
        SingletonResourceRepository<string, EventWaitHandle>.Get();
    private bool _disposed;
    private EventResetMode CurrentResetMode = EventResetMode.AutoReset;

    public event CallForInteraction? Name;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
