#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
#pragma warning disable 67

[Category("Printing & Formattin")]
[Title("Capture number in memory")]
[Description("""
             Turns an in-memory number into a counter, or makes a new number in memory to count
             with. Useful for numbering rows, pages, limiting output rows.
             
             When an underlying 'Delta' is triggered with a matching name, the captured number
             will be increased as specified by that Delta, in the entire underlying scope of
             this service.
             
             Also neat in conjunction with NumberBigger
             """)]
public class Number : IService
{
    [PrimarySetting("Memory name of number")]
    private readonly UpdatingPrimaryValue CounterNameConstant = new();
    [NamedSetting("initial", """
                             Initial value of number. Will override existing memory value in 
                             the underlying scope, if specified.
                             """)]
    private readonly UpdatingKeyValue InitialValueConstant = new("initial");

    private string? CurrentCounterName;
    private decimal CurrentInitialValue;
    
    [EventOccasion("Services under here may expect the number at the memory name to change according to Delta")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no name was provided")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, CounterNameConstant).IsRereadRequired(out string? counterNameCandidate))
            this.CurrentCounterName = counterNameCandidate;
        if ((constants, InitialValueConstant).IsRereadRequired(out decimal initialValueCandidate))
            this.CurrentInitialValue = initialValueCandidate;
        if (CurrentCounterName == null || string.IsNullOrWhiteSpace(CurrentCounterName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "counter name is required"));
            return;
        }

        decimal initialValue = this.CurrentInitialValue;
        if (interaction.TryFindVariable(CurrentCounterName, out object? existingNumberObject) &&
            existingNumberObject != null)
        {
            try
            {
                initialValue = Convert.ToDecimal(existingNumberObject);
            }
            catch (Exception)
            {
                // whatever
            }
        }

        var ci = new CounterInteraction(interaction, CurrentCounterName, initialValue);
        OnThen?.Invoke(this, ci);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}