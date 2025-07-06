namespace Ziewaar.RAD.Doodads.FormsValidation.Interactions;
#pragma warning disable 67
public class FieldPropertiesInteraction(
    IInteraction interaction,
    string Id,
    string Name,
    string Title,
    string Value) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => new SwitchingDictionary(
        ["id", "name", "title", "value"], x => x switch
        {
            "id" => Id,
            "name" => Name,
            "title" => Title,
            "value" => Value,
            _ => throw new KeyNotFoundException(),
        });
}