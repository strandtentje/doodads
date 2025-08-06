#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
[Category("Memory & Register")]
[Title("Put register value back into memory")]
[Description("""
             In effect, the opposite of Load; read its documentation.
             This takes the register value and puts it back into memory at the 
             configured name. Unless the `constant`-setting is specified; 
             then the configuration value is put into memory at the specified location.
             """)]
public class Store : IService
{
    [PrimarySetting("Key in memory to write to")]
    private readonly UpdatingPrimaryValue KeyConstant = new();
    [NamedSetting("constant", "Optional override value to use instead of the Register")]
    private readonly UpdatingKeyValue StoreValueConstant = new("constant");
    private string? KeyName;
    private object? DefaultValue;
    [EventOccasion("Having the register value in memory from here")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens because no key setting was provided")]
    public event CallForInteraction? OnException;
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
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}