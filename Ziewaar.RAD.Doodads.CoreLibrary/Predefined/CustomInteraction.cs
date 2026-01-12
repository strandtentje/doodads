#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class CustomInteraction<TType>(IInteraction interaction, TType payload) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public TType Payload => payload;
}