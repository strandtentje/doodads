#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Text in register")]
[Title("Check if register text ends with something")]
[Description("""
    Sinks an expression at Expression, and then validates the text in register
    against it. OnThen if matches, otherwise OnElse.
    """)]
public class EndsWith : IService
{
    private readonly UpdatingKeyValue IgnoreCaseConstant = new("ci");
    private bool IsCaseInsensitive;

    public event CallForInteraction? Expression;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, IgnoreCaseConstant).IsRereadRequired(out bool ci))
            this.IsCaseInsensitive = ci;

        var tsi = new TextSinkingInteraction(interaction);
        Expression?.Invoke(this, tsi);
        var sw = tsi.ReadAllText();
        if (interaction.Register.ToString().EndsWith(sw, IsCaseInsensitive ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture))
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
