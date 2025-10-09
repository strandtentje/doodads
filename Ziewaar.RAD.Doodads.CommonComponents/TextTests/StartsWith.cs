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
    [NamedSetting("trim", "true/false to indicate whether the tail must be trimmed")]
    private readonly UpdatingKeyValue TrimTailConstant = new("trim");

    private string? CurrentlySavingTailTo;
    private bool CurrentlyTrimming;

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
        if ((constants, TrimTailConstant).IsRereadRequired(
            () => false, out bool trimTail))
            this.CurrentlyTrimming = trimTail;

        var tsi = new TextSinkingInteraction(interaction);
        Expression?.Invoke(this, tsi);
        var sw = tsi.ReadAllText();
        if (string.IsNullOrWhiteSpace(CurrentlySavingTailTo) &&
            interaction.Register.ToString().StartsWith(sw))
            OnThen?.Invoke(this, interaction);
        else if (CurrentlySavingTailTo != null &&
                 interaction.Register.ToString().StartsWith(sw))
        {
            string tail = interaction.Register.ToString().Substring(sw.Length);
            if (CurrentlyTrimming) tail = tail.Trim();
            OnThen?.Invoke(this, new CommonInteraction(interaction,
                memory: new SortedList<string, object>() {
                    [CurrentlySavingTailTo] = tail,
                }));
        }
        else
            OnElse?.Invoke(this, interaction);
    }

    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}