#nullable enable
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler
{
#pragma warning disable 67
    public class ProfileByHeaviest : IService
    {
        private readonly UpdatingPrimaryValue RepeatNameConstant = new();
        private string? CurrentRepeatName;

        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
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
}