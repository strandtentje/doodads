#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Cooldown before next continue")]
[Description("""
    Will hand over the interaction provided the cooldown has elapsed.
    """)]
public class Cooldown : IService
{
    [PrimarySetting("Time in ms to cooldown with")]
    private readonly UpdatingPrimaryValue DelayInMsConst = new();
    private double CurrentDelay;
    private System.Threading.Timer? CurrentTimer;

    [EventOccasion("When the cooldown was cool")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("When the cooldown was hot")]
    public event CallForInteraction? OnException;

    public TimeSpan LastInvocation = TimeSpan.MinValue;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, DelayInMsConst).IsRereadRequired(out decimal number))
            this.CurrentDelay = (double)number;
        if ((constants, DelayInMsConst).IsRereadRequired(out string? reallyDude) && decimal.TryParse(reallyDude, out decimal actualDelay))
            this.CurrentDelay = (double)actualDelay;
        this.CurrentDelay = (double)Math.Max(this.CurrentDelay, 100);

        var currentTime = GlobalStopwatch.Instance.Elapsed;

        if (LastInvocation == TimeSpan.MinValue)
            LastInvocation = TimeSpan.Zero.Subtract(TimeSpan.FromMilliseconds(CurrentDelay + 1));

        var elapsedTime = currentTime - LastInvocation;

        if (elapsedTime.TotalMilliseconds > CurrentDelay)
        {
            LastInvocation = currentTime;
            OnThen?.Invoke(this, interaction);
        } else
        {
            OnElse?.Invoke(this, interaction);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
