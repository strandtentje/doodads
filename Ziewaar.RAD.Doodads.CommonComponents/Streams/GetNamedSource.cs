namespace Ziewaar.RAD.Doodads.Cryptography;

#nullable enable
public class GetNamedSource : IService
{
    private readonly UpdatingPrimaryValue SourceNameConstant = new();
    private string? CurrentSourceName;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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