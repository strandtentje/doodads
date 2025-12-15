#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Funnel multiple incoming events")]
[Description("""
             For smoothing out request peaks that have a higher rate
             than the underlying services can handle. ie. If ten events
             come in at once, will pass them on one-at-a-time and wait
             for the job to finish before passing on the next one.

             Unlike the regular hopper, this one blocks the calling branch
             until the called branch has had its turn and is done working.

             Needs Continue to continue distributing jobs; when no suitable
             Continue happened during a job, no further jobs will be processed.
             """)]
public class ChokingHopper : IService
{
    private SemaphoreSlim? CurrentChoke = null;
    [PrimarySetting("Number of tasks before choke occures")]
    private readonly UpdatingPrimaryValue ChokeConstant = new();
    [NamedSetting("timeout", "Timeout before we start doing work anyways")]
    private readonly UpdatingKeyValue TimeOutConstant = new("timeout");
    private readonly object ChokeLock = new();
    private int CurrentChokeCount = 5;
    private TimeSpan CurrentTimeout = TimeSpan.FromSeconds(3);
    [EventOccasion("The works done here")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
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