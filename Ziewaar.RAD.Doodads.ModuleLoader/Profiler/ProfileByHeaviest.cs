#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;
#pragma warning disable 67
[Category("Diagnostics & Debug")]
[Title("Sort through heaviest profiler entries")]
[Description("""
             Use with EnableProfiler and DisableProfiler to start 
             watching which services are hit the heaviest. Stops iterating
             when no more appropriate [Continue] is hit.
             """)]
public class ProfileByHeaviest : IService
{
    [PrimarySetting("Continue name")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName;

    [EventOccasion("""
                   Another profiler entry; the following is available in memory
                   "type" Typename of service
                   "file" File in which service is instantiated
                   "line" Line number of file
                   "pos" Character position in file
                   "time" Total time service has spent running
                   "active" Amount of currently active instances
                   "count" Total instance count
                   "state" State; "active" if one or more are running, "inactive if none"
                   "percentage" Percentage of doodads cpu time this service has taken
                   """)]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when no continue name was set")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
            this.CurrentRepeatName = repeatNameCandidate;
        if (string.IsNullOrWhiteSpace(CurrentRepeatName) || CurrentRepeatName == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "repeat name required as primary constant"));
            return;
        }

        var heaviest = ServiceProfiler.Instance.GetHeaviestRunningTotals().ToArray();
        var totalTime = heaviest.Aggregate(TimeSpan.Zero, (acc, item) => acc + item.time, result => result);
        var ri = new RepeatInteraction(CurrentRepeatName, interaction);
        foreach (var profile in heaviest)
        {
            if (!ri.IsRunning)
                return;
            ri.IsRunning = false;
            OnThen?.Invoke(this, new CommonInteraction(ri, new SwitchingDictionary(
                ["percentage", "type", "file", "line", "pos", "time", "active", "count", "state"], x => x switch
                {
                    "type" => profile.service.Typename,
                    "file" => profile.service.Filename,
                    "line" => profile.service.Line,
                    "pos" => profile.service.Position,
                    "time" => profile.time,
                    "active" => profile.activeCount,
                    "count" => profile.totalCount,
                    "state" => profile.activeCount > 0 ? "active" : "inactive",
                    "percentage" => Convert.ToDecimal((profile.time.Ticks * 100) / totalTime.Ticks),
                    _ => throw new KeyNotFoundException(),
                })));
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}