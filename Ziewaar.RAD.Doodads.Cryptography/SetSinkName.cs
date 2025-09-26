namespace Ziewaar.RAD.Doodads.Cryptography;

public class SetSinkName : IService
{
    private readonly UpdatingPrimaryValue SinkNameConstant = new();
    private string? CurrentSinkName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, SinkNameConstant).IsRereadRequired(out string? sourceNameCandidate))
            this.CurrentSinkName = sourceNameCandidate;
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