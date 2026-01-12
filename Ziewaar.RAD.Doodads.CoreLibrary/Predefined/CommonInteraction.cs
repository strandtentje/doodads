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
    public object Register { get; set; }
    public IReadOnlyDictionary<string, object> Memory { get; }
}