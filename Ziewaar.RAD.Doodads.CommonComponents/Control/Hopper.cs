#nullable enable
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control
{
    public class Hopper : IService, IDisposable
    {
        private readonly UpdatingPrimaryValue RepeatNameConstant = new();
        public event CallForInteraction? OnThen;
        public event CallForInteraction? OnElse;
        public event CallForInteraction? OnException;
        private readonly BlockingCollection<IInteraction> Jobs = new();
        private Task? Runner;
        private string? CurrentRepeatName;
        protected virtual bool IsLooping => false;

        public void Enter(StampedMap constants, IInteraction interaction)
        {
            if ((constants, RepeatNameConstant).IsRereadRequired(out string? repeatNameCandidate))
                this.CurrentRepeatName = repeatNameCandidate;
            if (string.IsNullOrWhiteSpace(this.CurrentRepeatName))
            {
                OnException?.Invoke(this, new CommonInteraction(interaction, "repeat name required"));
                return;
            }

            Jobs.Add(interaction);

            if (Runner == null || Runner.Status > TaskStatus.WaitingForChildrenToComplete)
            {
                try
                {
                    Runner?.Dispose();
                }
                catch (Exception ex)
                {
                    // whatever
                }

                Runner = Task.Run(() =>
                {
                    foreach (var item in Jobs.GetConsumingEnumerable())
                    {
                        var ri = new RepeatInteraction(this.CurrentRepeatName!, item);
                        ri.IsRunning = false;
                        OnThen?.Invoke(this, ri);
                        if (!ri.IsRunning) break;
                    }
                    OnElse?.Invoke(this, interaction);
                });
            }
        }

        public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);

        public void Dispose()
        {
            Jobs.CompleteAdding();
            try
            {
                Runner?.Dispose();
            }
            catch (Exception ex)
            {
                // whatever
            }
        }
    }
}