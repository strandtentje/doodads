#nullable enable
using System.Collections.Concurrent;
using System.Globalization;
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Scheduling & Flow")]
[Title("Tend to a long running task with an interval")]
[Description("""
    Provided an interval in seconds, will run a task until its done, wait the specified time, and do it again so long as the Continue.
    Works like a combination of Postpone and Repeat but blocks
    """)]
public class Recurring : IteratingService
{
    [NamedSetting("ms", "ms to delay with")]
    private readonly UpdatingKeyValue DelayInMsConstant = new("ms");
    private decimal CurrentDelay;
    private bool IsDisposing;

    protected override bool RunElse => false;
    [EventOccasion("Sink interval string here")]
    public override event CallForInteraction? OnElse;

    private readonly HashSet<SemaphoreSlim> ActiveSemaphores = new();

    protected override IEnumerable<IInteraction> GetItems(StampedMap constants, IInteraction repeater)
    {
        var tsi = new TextSinkingInteraction(repeater);
        OnElse?.Invoke(this, tsi);
        var timeText = tsi.ReadAllText();

        if (TimeSpan.TryParse(timeText, CultureInfo.InvariantCulture, out var ts))
            this.CurrentDelay = (decimal)ts.TotalMilliseconds;
        else if ((constants, DelayInMsConstant).IsRereadRequired(out decimal number))
            this.CurrentDelay = number;
        if ((constants, DelayInMsConstant).IsRereadRequired(out string? reallyDude) && decimal.TryParse(reallyDude, out decimal actualDelay))
            this.CurrentDelay = actualDelay;

        using (var semaphore = new SemaphoreSlim(0, 1))
        {
            lock (ActiveSemaphores)
            {
                if (IsDisposing)
                    yield break;
                ActiveSemaphores.Add(semaphore);
            }
            try
            {
                var ct = ((RepeatInteraction)repeater).CancellationToken;
                while (!ct.IsCancellationRequested && !IsDisposing)
                {
                    var waitingTask = semaphore.WaitAsync((int)this.CurrentDelay);
                    waitingTask.Wait(ct);
                    if (!ct.IsCancellationRequested && !IsDisposing)
                        yield return repeater;
                }
            }
            finally
            {
                lock (ActiveSemaphores)
                    if (!IsDisposing)
                        ActiveSemaphores.Remove(semaphore);
            }
        }
    }
    public override void Dispose()
    {
        Stack<SemaphoreSlim> toDispose;
        lock (ActiveSemaphores)
        {
            IsDisposing = true;
            toDispose = new Stack<SemaphoreSlim>(ActiveSemaphores);
        }
        while (toDispose.Count > 0)
        {
            var p = toDispose.Pop();
            try
            {
                p.Release();
            }
            catch (Exception)
            {
                // whatever
            }
            try
            {
                p.Dispose();
            }
            catch (Exception)
            {
                // whatever
            }
        }
        base.Dispose();
    }
}
