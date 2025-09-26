namespace Ziewaar.RAD.Doodads.Cryptography;

public class GetNamedSink : IService
{
    private readonly UpdatingPrimaryValue SinkNameConstant = new();
    private string? CurrentSinkName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
            OnException?.Invoke(this, new CommonInteraction(interaction, "sourcing interaction required"));
            return;
        }

        OnThen?.Invoke(this, new RecoveredSinkingInteraction(interaction, namedInteraction));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}