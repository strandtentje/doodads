#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;

[Category("Input & Validation")]
[Title("Memory to Stream")]
[Description("Make text in memory readable as a stream")]
public class SourceFrom : IService
{
    [PrimarySetting("Name in memory to source text from")]
    private readonly UpdatingPrimaryValue SourceVariableConstant = new();

    private string? CurrentSourceVariable;

    [EventOccasion("When text is available to source")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("Either no name was given, or no data was in memory")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, SourceVariableConstant).IsRereadRequired(out string? newSourceVariable) &&
            !string.IsNullOrWhiteSpace(newSourceVariable))
        {
            this.CurrentSourceVariable = newSourceVariable;
        }

        if (CurrentSourceVariable == null || string.IsNullOrWhiteSpace(CurrentSourceVariable))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "name of source variable is required"));
            return;
        }

        if (!interaction.TryFindVariable(CurrentSourceVariable, out object? sourceData) ||
            sourceData?.ToString() is not string sourceText)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "there was no data at the variable in memory"));
            return;
        }

        using (var tsi = new TextSourcingInteraction(interaction, sourceText))
        {
            OnThen?.Invoke(this, tsi);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}