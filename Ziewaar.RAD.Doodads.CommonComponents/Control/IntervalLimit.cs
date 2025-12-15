#nullable enable
#pragma warning disable 67
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Throttles a branch to be at least some amount of slow")]
[Description("""
    Does some work, and if its done quicker than the time specified in the primary 
    constant, it'll just hang around a bit to meet the time constant.
    """)]
public class IntervalLimit : IService
{
    [PrimarySetting("Amount of ms our work should at least take")]
    private readonly UpdatingPrimaryValue BlockConstant = new();
    private int CurrentBlockTime;
    [EventOccasion("Work to do.")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, BlockConstant).IsRereadRequired(out decimal newBlockTime))
            this.CurrentBlockTime = (int)newBlockTime;

        var startTime = GlobalStopwatch.Instance.ElapsedMilliseconds;
        OnThen?.Invoke(this, interaction);
        var spentTime = GlobalStopwatch.Instance.ElapsedMilliseconds - startTime;
        var compensationTime = Math.Max(1, Math.Min(1000000, CurrentBlockTime - spentTime)) ;
        GlobalLog.Instance?.Information("Job was done in {spenttime}ms so wasting {compensation}ms", spentTime, compensationTime);
        Thread.Sleep((int)compensationTime);
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
