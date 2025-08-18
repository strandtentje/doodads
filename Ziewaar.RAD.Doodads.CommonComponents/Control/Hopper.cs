#nullable enable
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;
[Category("Scheduling & Flow")]
[Title("Funnel multiple incoming events")]
[Description("""
             For smoothing out request peaks that have a higher rate
             than the underlying services can handle. ie. If ten events
             come in at once, will pass them on one-at-a-time and wait
             for the job to finish before passing on the next one.
             
             Needs Continue to continue distributing jobs; when no suitable
             Continue happened during a job, no further jobs will be processed.
             """)]
public class Hopper : IService, IDisposable
{
    [PrimarySetting("Continue name to use for green-lighting the next request")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    [EventOccasion("Hook up services here that need to be fed one-at-a-time.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("""
                   When no continue signal was given; hopper stops after this.
                   Jobs will be retained and feeding will resume when another job is added.
                   """)]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when no continue name was provided")]
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