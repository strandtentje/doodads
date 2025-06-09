#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class Store : IService
{
    private readonly UpdatingPrimaryValue KeyConstant = new();
    private readonly UpdatingKeyValue StoreValueConstant = new("constant");
    private string? KeyName;
    private object? DefaultValue;
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, KeyConstant).IsRereadRequired(out this.KeyName);
        (constants, StoreValueConstant).IsRereadRequired(out this.DefaultValue);
        if (KeyName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "key required as primary constant"));
            return;
        }

        SortedList<string, object> memory = new(1);
        if (DefaultValue == null)
            memory[KeyName] = interaction.Register;
        else
            memory[KeyName] = DefaultValue;
        OnThen?.Invoke(this, new CommonInteraction(interaction, memory: memory));
    }
}