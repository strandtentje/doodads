using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

#nullable enable

public class CommonInteraction(IInteraction interaction, object? register = null, SortedList<string, object>? memory = null) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register { get; } = register ?? interaction.Register;
    public IReadOnlyDictionary<string, object> Memory { get; } = memory ?? interaction.Memory;
}

public class RootInteraction(object register, SortedList<string, object> memory) : ISelfStartingInteraction
{
    public IInteraction Stack => StopperInteraction.Instance;
    public object Register => register;
    public IReadOnlyDictionary<string, object> Memory => memory;
}

public class StopperInteraction : IInteraction
{
    public static readonly StopperInteraction Instance = new();
    private StopperInteraction() { }
    public IInteraction Stack => throw new ArgumentOutOfRangeException("This is the stopper interaction.");
    public object Register => throw new ArgumentOutOfRangeException("This is the stopper interaction");
    public IReadOnlyDictionary<string, object> Memory => throw new ArgumentOutOfRangeException("This is the stopper interaction"); 
}
