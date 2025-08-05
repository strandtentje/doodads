#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Flow Control")]
[Title("Find the originating Repeat call, and invoke its children again.")]
[Description("""Read the docs on Repeat, for Continue will behave according to its definition. """)]
public class Continue : IService
{
    // TODO: Continue needs a shorthand like ["Text to Return to"]
    // TODO: Homogenize operations that output lists of things to Iterate with Continue instead of ienumerables or uncontrolled triggers
    // TODO: Continue will OnThen in case a Continue _can_ happen
    // TODO: Continue will OnElse if we're all out of items
    // TODO: Introduce Break to signal a premature iteration stop, explicitly. This will only:
    // TODO: Cause Continue's OnBreak to happen in case OnThen could have happened.
    // TODO: Change the shorthands . and , to : and | - the inconsistency is killing
    // TODO: Introduce a better design pattern to split the validations a service does from the actual work
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
