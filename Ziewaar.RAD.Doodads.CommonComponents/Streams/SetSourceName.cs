namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;
#pragma warning disable 67
#nullable enable
[Category("Sourcing & Sinking")]
[Title("Set name of current source")]
[Description("""
             For the sourcing stream that is currently in scope, set a name so it may 
             be retrieved in a deeper scope.
             """)]
public class SetSourceName : IService
{
    [PrimarySetting("Name for the stream.")]
    private readonly UpdatingPrimaryValue SourceNameConstant = new();
    private string? CurrentSourceName;
    [EventOccasion("Here, there sourcing stream lives under a name.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when there was no name or actual sourcing interaction")]
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

        if (!interaction.TryGetClosest<ISourcingInteraction>(out var sourcingInteraction) ||
            sourcingInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "sourcing interaction required"));
            return;
        }

        OnThen?.Invoke(this, new SourceNamingInteraction(interaction, this.CurrentSourceName, sourcingInteraction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}