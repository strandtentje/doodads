#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Text in register")]
[Title("Check if register text starts with something")]
[Description("""
    Sinks an expression at Expression, and then validates the text in register
    against it. OnThen if matches, otherwise OnElse.
    """)]
public class StartsWith : IService
{
    [EventOccasion("Sink an expression here that the register string should start with")]
    public event CallForInteraction? Expression;
    [EventOccasion("When the register string did indeed start with the expression")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the register string did not start with the expression")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        Expression?.Invoke(this, tsi);
        var sw = tsi.ReadAllText();
        if (interaction.Register.ToString().StartsWith(sw))
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
