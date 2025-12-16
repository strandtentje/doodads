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
public class Hopper : IteratingService, IDisposable
{
    private readonly UpdatingPrimaryValue RunnersConstant = new();
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
    public void Dispose()
    {
        Blocker.Dispose();
        Jobs.Dispose();
    }
}
