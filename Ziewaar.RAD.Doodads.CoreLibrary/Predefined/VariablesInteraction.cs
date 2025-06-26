namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

#nullable enable

public class CommonInteraction : IInteraction
{
    public CommonInteraction(IInteraction interaction, object? register = null, SortedList<string, object>? memory = null)
    {
        this.Stack = interaction;
        this.Register = register ?? interaction.Register;
        this.Memory = memory ?? interaction.Memory;
    }
    public CommonInteraction(IInteraction interaction, IReadOnlyDictionary<string, object>? memory, object? register = null)
    {
        this.Stack = interaction;
        this.Register = register ?? interaction.Register;
        this.Memory = memory ?? interaction.Memory;
    }
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
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
    [JsonIgnore]
    public IInteraction Stack => throw new ArgumentOutOfRangeException("This is the stopper interaction.");
    [JsonIgnore]
    public object Register => throw new ArgumentOutOfRangeException("This is the stopper interaction");
    [JsonIgnore]
    public IReadOnlyDictionary<string, object> Memory => EmptyReadOnlyDictionary.Instance;
}
