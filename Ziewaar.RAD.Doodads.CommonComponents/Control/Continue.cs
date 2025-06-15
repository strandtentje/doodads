#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Flow Control")]
[Title("Find the originating Repeat call, and invoke its children again.")]
[Description("""Read the docs on Repeat, for Continue will behave according to its definition. """)]
public class Continue : IService
{
    [PrimarySetting("Name of the Repeat block to fall back to.")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    [EventOccasion("Is never invoked; Continue is terminating for a block.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Is never invoked; Continue is terminating for a block.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens if the Repeat name was missing, or no Repeat with the configured name could be found.")]
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
