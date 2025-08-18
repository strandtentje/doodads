#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class Number : IService
{
    private readonly UpdatingPrimaryValue CounterNameConstant = new();
    private readonly UpdatingKeyValue InitialValueConstant = new("initial");

    private string? CurrentCounterName;
    private decimal CurrentInitialValue;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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
            catch (Exception ex)
            {
                // whatever
            }
        }

        var ci = new CounterInteraction(interaction, CurrentCounterName, initialValue);
        OnThen?.Invoke(this, ci);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}