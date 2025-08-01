#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
public class StoreLocale : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new LocaleInteraction(interaction, interaction.Register.ToString()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}