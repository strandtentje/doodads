#nullable enable
using System.IO.MemoryMappedFiles;

namespace Define.Content.AutomationKioskShell.ValidationNodes;
#pragma warning disable 67
[Category("Lists and Items")]
[Title("If a name doesn't exist in memory, set it to a default value from stream")]
[Description("""
             Provided a memory name, and a stream via OnThen, read either the memory value into
             register, or the stream data.
             """)]
public class Default : IService
{
    [PrimarySetting("Name of memory place to default a value for")]
    private readonly UpdatingPrimaryValue MemoryNameConst = new();
    [NamedSetting("set", "Fixed default value to use")]
    private readonly UpdatingKeyValue DefaultValueConst = new("set");
    private string? CurrentMemoryName;
    private object? FixedDefaultValue;
    [EventOccasion("Guaranteed to have a value of the specified name in register")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Text sink for default text value")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when the memory name has not been specified")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MemoryNameConst).IsRereadRequired(out string? newMemoryName))
            this.CurrentMemoryName = newMemoryName;
        if ((constants, DefaultValueConst).IsRereadRequired(out object? newFixedDefault))
            this.FixedDefaultValue = newFixedDefault;
        if (string.IsNullOrWhiteSpace(this.CurrentMemoryName) || this.CurrentMemoryName == null)
        {
            OnException?.Invoke(this, interaction);
            return;
        }
        if (!interaction.TryFindVariable<object>(this.CurrentMemoryName, out var memoryValue))
        {
            if (FixedDefaultValue != null && (FixedDefaultValue is not string fixedDefaultString ||
                                              !string.IsNullOrWhiteSpace(fixedDefaultString)))
            {
                memoryValue = FixedDefaultValue;
            }
            else
            {
                var tsi = new TextSinkingInteraction(interaction);
                OnElse?.Invoke(this, tsi);
                memoryValue = tsi.ReadAllText();
            }
        }
        OnThen?.Invoke(this, new CommonInteraction(interaction, register: memoryValue, memory: new SwitchingDictionary(
            [CurrentMemoryName], x => x == CurrentMemoryName ? memoryValue : throw new KeyNotFoundException())));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}