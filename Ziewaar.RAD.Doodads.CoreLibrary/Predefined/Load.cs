#pragma warning disable 67
#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
public class Load : IService
{
    private readonly UpdatingPrimaryValue KeyConstant = new();
    private readonly UpdatingKeyValue DefaultValueConstant = new("default");

    private string? KeyName;
    private object? DefaultValue;
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, KeyConstant).IsRereadRequired(out this.KeyName);
        (constants, DefaultValueConstant).IsRereadRequired(out this.DefaultValue);
        if (KeyName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "key required as primary constant"));
            return;
        }

        if (interaction.TryFindVariable(KeyName, out object? candidate) && candidate != null)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, candidate));
        }
        else
        {
            OnElse?.Invoke(this, new CommonInteraction(interaction, DefaultValue));
        }
    }
}