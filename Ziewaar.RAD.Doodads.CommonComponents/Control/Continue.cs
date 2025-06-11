#nullable enable

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class Continue : IService
{
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, RepeatNameConstant).IsRereadRequired(out string? repeatName);
        if (repeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "repeat name required"));
            return;
        }
        List<string> spottedRepeats = new(8);
        if (!interaction.TryGetClosest<RepeatInteraction>(out var ri, x => {
            spottedRepeats.Add(x.RepeatName);
            return x.RepeatName == repeatName;
        }) || 
            ri == null)
        {
            OnException?.Invoke(this, new CommonInteraction(
                interaction, $"could not find repeater with name {repeatName}; did you mean [{(string.Join(",", spottedRepeats))}]"));
            return;
        }
        if (ri.IsDeep)
            ri.ContinueFrom = interaction;
        ri.IsRunning = true;
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
