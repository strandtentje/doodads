#pragma warning disable 67
#nullable enable
namespace Define.Content.AutomationKioskShell.ValidationNodes;
public class Clamp : IService
{
    private IUpdatingValue
        DefaultValue = new UpdatingKeyValue("default"),
        MinValue = new UpdatingKeyValue("min"),
        MaxValue = new UpdatingKeyValue("max"),
        MinMaxRange = new UpdatingPrimaryValue();
    private decimal Min, Default, Max;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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