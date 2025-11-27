#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Repeat instructions while Continue is being encountered")]
[Description("""
             It's recommended not to use this, and always prefer recursing using the Call-method.
             Repeat won't loop unless it encounters a "Continue". Repeat will play nice with
             Calls to other Definitions, but it is hard to understand if it works. Only use locally.
             """)]
public class Repeat : IService
{
    [PrimarySetting("Description of this repeat block, must be the same for Repeat and related Continue")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();

    [NamedSetting("deep", "When Continue is encountered, bring its Buffer, Memory and Stack into the repetition.")]
    private readonly UpdatingKeyValue IsDeepConstant = new("deep");

    [EventOccasion("Logic to repeat hooks up to this")]
    public event CallForInteraction? OnThen;

    [EventOccasion(
        "If logic is hooked up here, it is called first. The repeat block will only repeat if this logic invokes a Continue")]
    public event CallForInteraction? OnElse;

    [EventOccasion("Likely happens when the repeat name is missing.")]
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

        var ri = new RepeatInteraction(repeatName, interaction) { IsDeep = mustGoDeep == true };
        OnElse?.Invoke(this, ri);
        while (ri.IsRunning)
        {
            ri.IsRunning = false;
            OnThen?.Invoke(this, ri.ContinueFrom ?? ri);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
