#nullable enable
using System.Threading;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

[Category("Flow Control")]
[Title("Tend to a long running task with an interval")]
[Description("""
    Provided an interval in seconds, will run a task until its done, wait the specified time, and do it again so long as the Continue.
    Works like a combination of Postpone and Repeat
    """)]
public class Maintain : IService, IDisposable
{
    private readonly HashSet<System.Threading.Timer> Timers = new();
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName = null;
    private bool IsDisposing;

    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RepeatNameConstant).IsRereadRequired(out string? newRepeatName) && newRepeatName != null)
            this.CurrentRepeatName = newRepeatName;
        if (this.CurrentRepeatName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "repeat name is required"));
            return;
        }
        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        if (TimeSpan.TryParse(tsi.ReadAllText(), out var timespanResult))
        {
            try
            {
                foreach (var item in Timers)
                {
                    item.Dispose();
                }
            }
            catch (Exception)
            { 
                // its fine
            }
            finally
            {
                Timers.Clear();
            }
            StartMaintaining(new RepeatInteraction(this.CurrentRepeatName, interaction), timespanResult);
        }
    }

    private void StartMaintaining(RepeatInteraction interaction, TimeSpan interval)
    {
        System.Threading.Timer? timer = null;
        timer = new System.Threading.Timer(_ =>
        {
            timer!.Change(Timeout.Infinite, Timeout.Infinite);
            Timers.Remove(timer!);
            interaction.IsRunning = false;
            OnThen?.Invoke(this, interaction);
            if (!interaction.IsRunning)
                return;
            timer.Change((int)(Math.Min(int.MaxValue, interval.TotalMilliseconds)), Timeout.Infinite);
            Timers.Add(timer!);
        }, null, Timeout.Infinite, Timeout.Infinite);
        Timers.Add(timer);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        if (!IsDisposing)
        {
            IsDisposing = true;
            foreach (var item in this.Timers)
            {
                item.Dispose();
            }
        }
    }
}
