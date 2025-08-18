#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class Delta : IService
{
    private readonly UpdatingPrimaryValue CounterNameConstant = new();
    private readonly UpdatingKeyValue IncrementValueConstant = new("by");
    private string? CurrentCounterName;
    private decimal CurrentIncrementValue;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, CounterNameConstant).IsRereadRequired(out string? counterNameCandidate))
            this.CurrentCounterName = counterNameCandidate;
        if ((constants, IncrementValueConstant).IsRereadRequired(out decimal initialValueCandidate))
            this.CurrentIncrementValue = initialValueCandidate;
        if (CurrentCounterName == null || string.IsNullOrWhiteSpace(CurrentCounterName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "counter name is required"));
            return;
        }
        if (!interaction.TryGetClosest<CounterInteraction>(out CounterInteraction? counterInteraction,
                x => x.Name == this.CurrentCounterName) || counterInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "couldn't find counter with name"));
            return;
        }
        counterInteraction.Add(this.CurrentIncrementValue);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}