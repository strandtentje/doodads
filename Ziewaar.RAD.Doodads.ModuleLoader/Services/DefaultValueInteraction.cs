using Ziewaar.RAD.Doodads.CoreLibrary;

#pragma warning disable 67
#nullable enable

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;

public class DefaultValueInteraction(IInteraction interaction, SortedList<string, object> memory) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = new InteractingDefaultingDictionary(real: interaction, fallback: memory);
}
