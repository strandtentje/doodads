#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

#pragma warning disable 67

[Category("Printing & Formatting")]
[Title("Increase number in Memory")]
[Description("""
             For a number in memory that has been captured by the Number service,
             increase it by a fixed amount. The increased number is then available 
             under the Number service with the matching name.
             """)]
public class Delta : IService
{
    [PrimarySetting("Name of the number as captured by Number, to increment")]
    private readonly UpdatingPrimaryValue CounterNameConstant = new();
    [NamedSetting("by", "Amount to increase with (or decrease, use negative). Decimals allowed.")]
    private readonly UpdatingKeyValue IncrementValueConstant = new("by");
    private string? CurrentCounterName;
    private decimal CurrentIncrementValue;
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When no counter name was provided, or when no counter with the specified name was found")]
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
        if (!interaction.TryGetClosest(out CounterInteraction? counterInteraction,
                x => x.Name == this.CurrentCounterName) || counterInteraction == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "couldn't find counter with name"));
            return;
        }
        counterInteraction.Add(this.CurrentIncrementValue);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}