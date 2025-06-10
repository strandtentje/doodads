#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
public class Release : IService
{
    private readonly UpdatingPrimaryValue LockNameConstant = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? Name;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LockNameConstant).IsRereadRequired(out string? lockName);
        var nameSource = new TextSinkingInteraction(interaction);
        Name?.Invoke(this, interaction);
        string? desiredName = lockName;
        using (var rd = nameSource.GetDisposingSinkReader())
        {
            desiredName ??= rd.ReadToEnd();
        }
        if (string.IsNullOrWhiteSpace(desiredName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Hold Lock required name"));
            return;
        }
        if (interaction.TryGetClosest<ResidentialInteraction>(out var candidate, x => x.Name == desiredName) && 
            candidate != null)
        {
            candidate.Leave();
            OnThen?.Invoke(this, interaction);
        } else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cannot terminate non-existent residence."));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
