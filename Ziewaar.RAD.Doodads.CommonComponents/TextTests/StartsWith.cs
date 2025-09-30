#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextTests;

[Category("Input & Validation")]
[Title("Check if register text starts with something")]
[Description("""
             Sinks an expression at Expression, and then validates the text in register
             against it. OnThen if matches, otherwise OnElse.
             """)]
public class StartsWith : IService
{
    [PrimarySetting(
        "Variable to write remainder of matching text into " +
        "(after the startswith string)")]
    private readonly UpdatingPrimaryValue TailVariableConstant = new();

    private string? CurrentlySavingTailTo;

    [EventOccasion(
        "Sink an expression here that the register string should start with")]
    public event CallForInteraction? Expression;

    [EventOccasion(
        "When the register string did indeed start with the expression")]
    public event CallForInteraction? OnThen;

    [EventOccasion(
        "When the register string did not start with the expression")]
    public event CallForInteraction? OnElse;

    [NeverHappens] public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, TailVariableConstant).IsRereadRequired(
                out string? tailVarCandidate))
            this.CurrentlySavingTailTo = tailVarCandidate;

        var tsi = new TextSinkingInteraction(interaction);
        Expression?.Invoke(this, tsi);
        var sw = tsi.ReadAllText();
        if (string.IsNullOrWhiteSpace(CurrentlySavingTailTo) &&
            interaction.Register.ToString().StartsWith(sw))
            OnThen?.Invoke(this, interaction);
        else if (CurrentlySavingTailTo != null &&
                 interaction.Register.ToString().StartsWith(sw))
            OnThen?.Invoke(this,
                new CommonInteraction(interaction,
                    memory: new SortedList<string, object>()
                    {
                        [CurrentlySavingTailTo] = interaction.Register
                            .ToString().Substring(sw.Length),
                    }));
        else
            OnElse?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}