#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;
[Category("Memory & Register")]
[Title("Conditional branch on register value")]
[Description("""
             Checks if the register's string-converted value is an exact match with the provided primary
             setting text. 
             """)]
public class Case : IService
{
    [PrimarySetting("String value to match the Register against")]
    private readonly UpdatingPrimaryValue MatchStringConst = new();
    [EventOccasion("When the string representation of the register matches the setting")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When there was no match")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, MatchStringConst).IsRereadRequired(out string? matchString);

        var directStringToMatch =
            interaction.Register is string value ? value : Convert.ToString(interaction.Register);

        if (directStringToMatch == matchString)
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}