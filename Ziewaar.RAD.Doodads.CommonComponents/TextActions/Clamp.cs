#pragma warning disable 67
#nullable enable
namespace Define.Content.AutomationKioskShell.ValidationNodes;
[Category("Validation")]
[Title("Clamp numeric value in registry to a range")]
[Description("""
             Does a best effort at turning the Register contents into a decimal number,
             and attempt to limit that numeric value between min and max. If the value
             cannot be retrieved, default is used.
             """)]
public class Clamp : IService
{
    [NamedSetting("default", "Default numeric value to fall back to.")]
    private readonly UpdatingKeyValue DefaultValue = new ("default");
    [NamedSetting("min", "Minimum numeric value to clamp to")]
    private readonly UpdatingKeyValue MinValue = new("min");
    [NamedSetting("max", "Maximum numeric value to clamp to")]
    private readonly UpdatingKeyValue MaxValue = new("max");
    [PrimarySetting("""
                    An expression of numbers with one or two <'s ie. 0<5<10. Outer numbers go to min/max, middle number 
                    to default. When a single value is provided, 0<0<x is presumed.
                    """)]
    private readonly UpdatingPrimaryValue MinMaxRange = new ();
    private decimal Min, Default, Max;
    [EventOccasion("After the clamped value was put into register")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MinMaxRange).IsRereadRequired(sourceDefault: () =>
            {
                (constants, MinValue).IsRereadRequired(() => 0M, out var minValue);
                (constants, MaxValue).IsRereadRequired(() => 1M, out var maxValue);
                (constants, DefaultValue).IsRereadRequired(() => 0M, out var defaultValue);
                return $"{minValue:0.00}<{defaultValue:0.00}<{maxValue:0.00}";
            }, out string? range))
        {
            var rangeNumeric = range?.Split('<').Select(decimal.Parse).ToArray();
            if (rangeNumeric?.Length >= 3)
            {
                this.Min = rangeNumeric[0];
                this.Default = rangeNumeric[1];
                this.Max = rangeNumeric[2];
            }
            else if (rangeNumeric?.Length == 2)
            {
                this.Min = this.Default = rangeNumeric[0];
                this.Max = rangeNumeric[1];
            }
            else if (rangeNumeric?.Length == 1)
            {
                this.Min = this.Default = 0M;
                this.Max = rangeNumeric[0];
            }
            else
            {
                this.Min = this.Default = 0M;
                this.Max = 1M;
            }
        }

        try
        {
            var toClamp = Convert.ToDecimal(interaction.Register);
            OnThen?.Invoke(this, new CommonInteraction(interaction, Math.Max(Math.Min(toClamp, Max), Min)));
        }
        catch (Exception)
        {
            OnThen?.Invoke(this, new CommonInteraction(interaction, DefaultValue));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}