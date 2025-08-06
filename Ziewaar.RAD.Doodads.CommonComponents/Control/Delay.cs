#nullable enable
#pragma warning disable 67
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Delay continuing of execution")]
[Description("""
    pass a fixed value to the primary parameter. execution of onthen will delay by that time in ms.
    """)]
public class Delay : IService
{
    [PrimarySetting("Time to delay with")]
    private readonly UpdatingPrimaryValue DelayInMsConest = new();
    private decimal CurrentDelay;

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
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Thread.Sleep((int)this.CurrentDelay);
            OnThen?.Invoke(this, interaction);
        });
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
