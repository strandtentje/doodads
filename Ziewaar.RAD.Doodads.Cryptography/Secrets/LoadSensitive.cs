using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

namespace Ziewaar.RAD.Doodads.Cryptography;
[Category("Tokens & Cryptography")]
[Title("Load string as sensitive")]
[Description("""
             Find a string by name in memory, make it unreadable and save it for
             single use as a sensitive interaction.
             """)]
public class LoadSensitive : IService
{
    [PrimarySetting("Name in memory")] private readonly UpdatingPrimaryValue NameConstant = new();
    private string? CurrentName;
    [EventOccasion("When the string was found and protected from repeated reading")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("When the name was missing or no value was found")]
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