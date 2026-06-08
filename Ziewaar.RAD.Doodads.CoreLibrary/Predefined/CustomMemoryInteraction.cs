#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class CustomMemoryInteraction<TCustomMemory>(IInteraction interaction) : IInteraction where TCustomMemory : IReadOnlyDictionary<string, object>, new()
{
    public IInteraction Stack => interaction.Stack;
    public object Register { get; set; } = interaction.Register;
    public readonly TCustomMemory CustomMemory = new();
    public IReadOnlyDictionary<string, object> Memory => CustomMemory;
}
