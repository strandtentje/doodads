namespace Ziewaar.RAD.Doodads.Cryptography;
public class LoadSensitive : IService
{
    private readonly UpdatingPrimaryValue NameConstant = new();
    private string? CurrentName;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, NameConstant).IsRereadRequired(out string? newName))
            this.CurrentName = newName;
        if (string.IsNullOrWhiteSpace(this.CurrentName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "name required"));
            return;
        }
        if (!interaction.TryFindVariable(this.CurrentName, out object? value) ||
            value?.ToString() is not string sensitiveString)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no value found"));
            return;
        }
        OnThen?.Invoke(this, new ExpiringStringInteraction(interaction, CurrentName, sensitiveString));
    }
    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, new CommonInteraction(source, ex));
}