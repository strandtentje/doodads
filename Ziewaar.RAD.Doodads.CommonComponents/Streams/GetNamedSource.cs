namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;
#pragma warning disable 67

#nullable enable
[Category("Sourcing & Sinking")]
[Title("Get source by name")]
[Description("""
             For a source that was previously given a name within this scope,
             bring it back as the main scope source again.
             """)]
public class GetNamedSource : IService
{
    [PrimarySetting("Name given to the source")]
    private readonly UpdatingPrimaryValue SourceNameConstant = new();
    private string? CurrentSourceName;

    [EventOccasion("Here, the source has been recovered.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no name was specified, or no source was given that name.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, SourceNameConstant).IsRereadRequired(out string? sourceNameCandidate))
            this.CurrentSourceName = sourceNameCandidate;
        if (string.IsNullOrWhiteSpace(this.CurrentSourceName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "source name required as primary constant"));
            return;
        }

        if (!interaction.TryGetClosest<SourceNamingInteraction>(out var namedInteraction,
                x => x.SourceName == this.CurrentSourceName) || namedInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "sourcing interaction required"));
            return;
        }

        OnThen?.Invoke(this, new RecoveredSourcingInteraction(interaction, namedInteraction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}