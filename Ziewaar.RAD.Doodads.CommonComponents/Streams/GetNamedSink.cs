namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;
#pragma warning disable 67
#nullable enable
[Category("Sourcing & Sinking")]
[Title("Get sink by name")]
[Description("""
             For a sink that was previously given a name within this scope,
             bring it back as the main scope sink again.
             """)]
public class GetNamedSink : IService
{
    [PrimarySetting("Name given to the sink")]
    private readonly UpdatingPrimaryValue SinkNameConstant = new();
    private string? CurrentSinkName;
    [EventOccasion("Here, the sink has been recovered.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no name was specified, or no sink was given that name.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, SinkNameConstant).IsRereadRequired(out string? sinkNameCandidate))
            this.CurrentSinkName = sinkNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentSinkName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "sink name required as primary constant"));
            return;
        }

        if (!interaction.TryGetClosest<SinkNamingInteraction>(out var namedInteraction,
                x => x.SinkName == this.CurrentSinkName) || namedInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "naming interaction required"));
            return;
        }

        OnThen?.Invoke(this, new RecoveredSinkingInteraction(interaction, namedInteraction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}