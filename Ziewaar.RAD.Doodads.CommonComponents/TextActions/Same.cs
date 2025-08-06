#pragma warning disable 67
#nullable enable
using System.Collections;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Parsing & Composing")]
[Title("Compare two memroy values")]
[Description("""
             Compares string representations of memory values.
             """)]
public class Same : IService
{
    [PrimarySetting("constant names in memory, may be string with commas, or array of strings")]
    private readonly UpdatingPrimaryValue MemoryNamesConst = new();
    [NamedSetting("onempty", "what to do if there were no values (else or then)")]
    private readonly UpdatingKeyValue OnEmptyConstant = new UpdatingKeyValue("onempty");
    private string[] FixedMemoryNames = [];
    private string OnEmptyDo = "else";
    [EventOccasion("Get names dynamically (print with comma separation)")]
    public event CallForInteraction? GetNames;
    [EventOccasion("when all names were thesame")]
    public event CallForInteraction? OnThen;
    [EventOccasion("when some names were different")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MemoryNamesConst).IsRereadRequired(out object? names))
        {
            if (names is string stringNames)
                this.FixedMemoryNames = stringNames.Split([','], (StringSplitOptions)3);
            else if (names is IEnumerable arrayNames)
                this.FixedMemoryNames = arrayNames.OfType<string>().ToArray();
        }
        if ((constants, OnEmptyConstant).IsRereadRequired(out string? whatOnEmpty))
            this.OnEmptyDo = whatOnEmpty?.Trim().ToLower() == "then" ? "then" : "else";

        string[] MemoryNames;

        if (this.FixedMemoryNames.Length == 0)
        {
            var tsi = new TextSinkingInteraction(interaction);
            GetNames?.Invoke(this, tsi);
            MemoryNames = tsi.ReadAllText().Split(',');
        }
        else
        {
            MemoryNames = this.FixedMemoryNames;
        }

        var values = MemoryNames.
            Select(x => (isPresent: interaction.TryFindVariable(x, out object? value) && value != null, foundText: value?.ToString())).ToArray();

        if (values.Length == 0 && this.OnEmptyDo == "then")
        {
            OnThen?.Invoke(this, interaction);
        }
        else if (
            values .Any() && 
            values.All(x => x.isPresent) && 
            values.Select(x => x.foundText).OfType<string>().Distinct().Count() == 1)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, values.ElementAt(0).foundText));
        }
        else
        {
            OnElse?.Invoke(this, new CommonInteraction(interaction, values.ElementAt(0).foundText));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex)
    {
        throw new NotImplementedException();
    }
}
