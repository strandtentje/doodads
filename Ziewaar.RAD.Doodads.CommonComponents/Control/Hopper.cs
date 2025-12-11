#nullable enable
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
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
            catch (Exception)
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
        catch (Exception)
        {
            // whatever
        }
    }
}

public class ChokingHopper : IService
{
    private SemaphoreSlim? CurrentChoke = null;
    private readonly UpdatingPrimaryValue ChokeConstant = new();
    private readonly UpdatingKeyValue TimeOutConstant = new("timeout");
    private readonly object ChokeLock = new();
    private int CurrentChokeCount = 5;
    private TimeSpan CurrentTimeout = TimeSpan.FromSeconds(3);
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, ChokeConstant).IsRereadRequired(() => 5, out decimal chokeValue) && chokeValue > 0)
        {
            lock (ChokeLock)
            {
                if (CurrentChokeCount != chokeValue)
                {
                    CurrentChoke?.Dispose();
                    CurrentChoke = null;
                }

                CurrentChokeCount = (int)chokeValue;
            }
        }

        if ((constants, TimeOutConstant).IsRereadRequired(() => 3000, out decimal timeoutInMs))
            CurrentTimeout = TimeSpan.FromMilliseconds((double)timeoutInMs);
        
        CurrentChoke ??= new SemaphoreSlim(CurrentChokeCount, CurrentChokeCount);

        SemaphoreSlim? toRelease = null;
        try
        {
            if (CurrentChoke.Wait(this.CurrentTimeout))
                toRelease = CurrentChoke;
        }
        catch (ObjectDisposedException ex)
        {
            // its ok.
        }

        try
        {
            OnThen?.Invoke(this, interaction);
        }
        finally
        {
            try
            {
                toRelease?.Release();
            }
            catch (ObjectDisposedException ex)
            {
                // its ok.
            }
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}