#nullable enable
#pragma warning disable 67
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Flow Control")]
[Title("Interval repetition")]
[Description("Repeat an action while continue")]
public class Interval : IService
{
    [PrimarySetting("Name for continue")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    [NamedSetting("ms", "ms to delay with")]
    private readonly UpdatingKeyValue DelayInMsConstant = new("ms");
    private decimal CurrentDelay;
    private System.Threading.Timer? CurrentTimer;
    private string? RepeatName;

    [EventOccasion("When the delay expires")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when repeat name was forgotten")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatName))
            this.RepeatName = repeatName;
        if ((constants, DelayInMsConstant).IsRereadRequired(out decimal number))
            this.CurrentDelay = number;
        if ((constants, DelayInMsConstant).IsRereadRequired(out string? reallyDude) && decimal.TryParse(reallyDude, out decimal actualDelay))
            this.CurrentDelay = actualDelay;
        if (string.IsNullOrWhiteSpace(RepeatName) || RepeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Repeat name required"));
            return;
        }

        var ri = new RepeatInteraction(this.RepeatName, interaction)
        {
            IsRunning = true
        };

        this.CurrentDelay = Math.Max(this.CurrentDelay, 100);

        this.CurrentTimer?.Dispose();
        this.CurrentTimer = new(_ =>
        {
            ri.IsRunning = false;
            OnThen?.Invoke(this, ri);
            if (!ri.IsRunning)
                this.CurrentTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        }, null, (int)this.CurrentDelay, (int)this.CurrentDelay);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
