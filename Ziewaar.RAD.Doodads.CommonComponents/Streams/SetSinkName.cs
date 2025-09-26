namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;
#pragma warning disable 67
#nullable enable
[Category("Sourcing & Sinking")]
[Title("Set name of current sink")]
[Description("""
             For the sinking stream that is currently in scope, set a name so it may 
             be retrieved in a deeper scope.
             """)]
public class SetSinkName : IService
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

        if (!interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) || sinkingInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "sink interaction required"));
            return;
        }

        OnThen?.Invoke(this, new SinkNamingInteraction(interaction, this.CurrentSinkName, sinkingInteraction));
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}