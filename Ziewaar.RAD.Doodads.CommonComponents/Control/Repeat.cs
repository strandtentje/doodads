#nullable enable

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class Repeat : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private readonly UpdatingKeyValue IsDeepConstant = new("deep");
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, RepeatNameConstant).IsRereadRequired(out string? repeatName);
        (constants, IsDeepConstant).IsRereadRequired(out bool? mustGoDeep);
        if (repeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "repeat name required"));
            return;
        }
        var ri = new RepeatInteraction(repeatName, interaction)
        {
            IsDeep = mustGoDeep == true
        };
        OnElse?.Invoke(this, ri);
        while(ri.IsRunning)
        {
            ri.IsRunning = false;
            OnThen?.Invoke(this, ri.ContinueFrom ?? ri);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
