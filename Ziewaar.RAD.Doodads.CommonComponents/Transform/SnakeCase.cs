#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.TextActions;

[Category("Parsing & Composing")]
[Title("Make text snakecase safe")]
[Description("""
    Turns a string of text with lEttERs and numb3rs into snakecase,  such that it becomes:
    turns_a_string_of_text_with_letters_and_numb_rs_into_snakecase_such_that_it_becomes
    - Makes all lower case
    - Replaces non alphanumeric with underscores
    - Cleans up double underscores
    - Trims trailing and leading underscores
    """)]
public class SnakeCase : IService
{
    [PrimarySetting("Set a variable name here to not snakify the register but a memory item instead")]
    private readonly UpdatingPrimaryValue VariableNameConstant = new();
    private string? VariableName = null;

    [EventOccasion("Snake comes out here in register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when register contents could not be turned into string")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, VariableNameConstant).IsRereadRequired(out string? nameToSnake) && !string.IsNullOrWhiteSpace(nameToSnake))
        {
            VariableName = nameToSnake;
        }
                
        try
        {
            if (VariableName == null)
            {
                var unsnakedObject = interaction.Register;
                string trimmedSnakeText = Snakify(unsnakedObject);
                OnThen?.Invoke(this, new CommonInteraction(interaction, trimmedSnakeText));
            } else if (interaction.TryFindVariable(VariableName, out object? unsnakedObject) && unsnakedObject != null)
            {
                string trimmedSnakeText = Snakify(unsnakedObject);
                OnThen?.Invoke(
                    this,
                    new CommonInteraction(
                        interaction,
                        new SwitchingDictionary([VariableName],
                        key => key == VariableName ?
                        trimmedSnakeText :
                        throw new KeyNotFoundException())));
            }
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, ex));
        }
    }

    private static string Snakify(object unsnakedObject)
    {
        var unsnakedText = unsnakedObject.ToString();
        var lowerText = unsnakedText.ToLower();
        var longSnakeText = new string([.. lowerText.Select(x => char.IsLower(x) || char.IsNumber(x) ? x : '_')]);
        while (longSnakeText.Contains("__"))
            longSnakeText = longSnakeText.Replace("__", "_");
        var trimmedSnakeText = longSnakeText.Trim('_');
        return trimmedSnakeText;
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
