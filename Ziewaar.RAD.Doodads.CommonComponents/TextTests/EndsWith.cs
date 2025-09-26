#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;
[Category("Input & Validation")]
[Title("Check if register text ends with something")]
[Description("""
             Sinks an expression at Expression, and then validates the text in register
             against it. OnThen if matches, otherwise OnElse.
             """)]
public class EndsWith : IService
{
    [NamedSetting("ci", "Ignore case true/false")]
    private readonly UpdatingKeyValue IgnoreCaseConstant = new("ci");
    private bool IsCaseInsensitive;
    [EventOccasion("Sink an expression here that the register string should end with")]
    public event CallForInteraction? Expression;
    [EventOccasion("When the register string did indeed end with the expression")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When the register string did not end with the expression")]
    public event CallForInteraction? OnElse;
    [NeverHappens] public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, IgnoreCaseConstant).IsRereadRequired(out bool ci))
            this.IsCaseInsensitive = ci;

        var tsi = new TextSinkingInteraction(interaction);
        Expression?.Invoke(this, tsi);
        var sw = tsi.ReadAllText();
        if (interaction.Register.ToString().EndsWith(sw,
                IsCaseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}