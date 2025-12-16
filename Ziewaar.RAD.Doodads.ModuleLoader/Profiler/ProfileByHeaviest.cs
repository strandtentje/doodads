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
             
             OnThen; the following is available in memory
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
public class ProfileByHeaviest : IteratingService
{
    protected override bool RunElse { get; }
    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        var heaviest = ServiceProfiler.Instance.GetHeaviestRunningTotals().ToArray();
        var totalTime = heaviest.Aggregate(TimeSpan.Zero, (acc, item) => acc + item.time, result => result);
        return heaviest.Select(profile => new CommonInteraction(repeater, new SwitchingDictionary(
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