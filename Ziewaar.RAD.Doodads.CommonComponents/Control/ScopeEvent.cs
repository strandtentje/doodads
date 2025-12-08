#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class ScopeEvent : IService
{
    private static readonly UpdatingPrimaryValue ResetModeConstant = new();
    public static readonly SingletonResourceRepository<string, EventWaitHandle> EwhRepo =
        SingletonResourceRepository<string, EventWaitHandle>.Get();
    private bool _disposed;
    private EventResetMode CurrentResetMode = EventResetMode.ManualReset;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ResetModeConstant).IsRereadRequired(out string? resetModeCandidate) &&
            resetModeCandidate != null)
            this.CurrentResetMode = (EventResetMode)Enum.Parse(typeof(EventResetMode), resetModeCandidate);

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var ewhName = tsi.ReadAllText();
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
