#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
[Category("Templating")]
[Title("Set current locale string")]
[Description("""
             Takes a string from the register and marks it as the current locale string.
             """)]
public class StoreLocale : IService
{
    [EventOccasion("Continues with locale here.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]    
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new LocaleInteraction(interaction, interaction.Register.ToString()));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}