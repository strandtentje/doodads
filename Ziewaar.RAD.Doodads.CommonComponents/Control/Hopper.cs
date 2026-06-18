using System.Collections.Concurrent;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Spin up workers")]
[Description("""
             Spread out inputs that come in at great rate, over several workers, such that multiple
             threads can do work.
             """)]
public class Workers : IService, IDisposable
{
    public Workers() => (new Thread(WorkerLoop)).Start(true);
    [EventOccasion("Attack worker here")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When work ran out and workers cooled down")]
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    [PrimarySetting("Worker count")]
    private readonly UpdatingPrimaryValue WorkerCount = new();
    
    private readonly BlockingCollection<IInteraction> Jobs = new();
    private readonly Semaphore JobOverflowPrevention = new(64, 64); 
    private int CurrentWorkers = 0;
    private int CalledForWorkers = 1;
    private bool IsDisposing;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        this.CalledForWorkers = Convert.ToInt32(constants.PrimaryConstant);
        JobOverflowPrevention.WaitOne();
        Jobs.Add(interaction);
    }

    private void WorkerLoop(object arg)
    {
        Interlocked.Increment(ref CurrentWorkers);
        IInteraction? lastSeen = null;
        try
        {
            while (true)
            {
                if (Jobs.TryTake(out var item, 3072))
                {
                    try
                    {
                        OnThen?.Invoke(this, lastSeen = item);
                    }
                    finally
                    {
                        JobOverflowPrevention.Release();
                    }
                }
                else if (arg is not bool b || b == false || this.IsDisposing)
                    return;
                else if (Jobs.Any())
                    for (int i = 0; i < CalledForWorkers - CurrentWorkers; i++)
                        (new Thread(WorkerLoop)).Start(null);
            }
        }
        finally
        {
            if (arg is bool b && b == true && IsDisposing)
            {
                JobOverflowPrevention.Dispose();
            }
            if (Interlocked.Decrement(ref CurrentWorkers) == 1 && lastSeen != null)
            {
                OnElse?.Invoke(this, lastSeen);
            }
        }
    }
    
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        Jobs.CompleteAdding();
        this.IsDisposing = true;
    }
}

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
public class Hopper : IteratingService, IDisposable
{
    [NamedSetting("runners", "Amount of parallel jobs that can come from the hopper")]
    private readonly UpdatingKeyValue RunnersConstant = new("runners");
    private int CurrentAmountOfRunners = 1;
    private SemaphoreSlim Blocker = new(1, 1);
    private readonly BlockingCollection<IInteraction> Jobs = new();
    protected virtual bool IsLooping => false;
    protected override bool RunElse => false;

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        if ((constants, RunnersConstant).IsRereadRequired(() => 1, out int countCandidate))
        {
            if (countCandidate != CurrentAmountOfRunners)
            {
                Blocker.Dispose();
                Blocker = new SemaphoreSlim(CurrentAmountOfRunners, CurrentAmountOfRunners);
                CurrentAmountOfRunners = countCandidate;
            }
        }

        Jobs.Add(repeater);
        Blocker.Wait(repeater.GetCancellationToken());
        try
        {
            while (!repeater.IsCancelled())
                if (Jobs.TryTake(out var job, 1000, repeater.GetCancellationToken()))
                    yield return job;
        }
        finally
        {
            Blocker.Release();
        }
    }

    protected override IEnumerable<IInteraction> GetElseItems(StampedMap constants, IInteraction repeater) => [];

    public override void Dispose()
    {
        base.Dispose();
        Blocker.Dispose();
        Jobs.Dispose();
    }
}