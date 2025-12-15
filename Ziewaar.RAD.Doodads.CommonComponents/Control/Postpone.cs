#nullable enable
#pragma warning disable 67
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Postpone continuing of execution")]
[Description("""
    pass a fixed value to the primary parameter. execution of onthen will delay by that time in ms; 
    repeated invocations will postpone further instead of causing repeated delays.
    """)]
public class Postpone : IService
{
    [PrimarySetting("Time to delay with")]
    private readonly UpdatingPrimaryValue DelayInMsConest = new();
    private decimal CurrentDelay;
    private System.Threading.Timer? CurrentTimer;

    [EventOccasion("When the delay expires")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DelayInMsConest).IsRereadRequired(out decimal number))
            this.CurrentDelay = number;
        if ((constants, DelayInMsConest).IsRereadRequired(out string? reallyDude) && decimal.TryParse(reallyDude, out decimal actualDelay))
            this.CurrentDelay = actualDelay;
        this.CurrentDelay = Math.Max(this.CurrentDelay, 100);

        this.CurrentTimer?.Dispose();
        this.CurrentTimer = new(_ =>
        {
            OnThen?.Invoke(this, interaction);
        }, null, (int)this.CurrentDelay, Timeout.Infinite);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
