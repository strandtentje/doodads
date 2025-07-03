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
    private readonly HashSet<Thread> Timers = new();
    [PrimarySetting("Use this name to explain what is being repeated. Use in conjunction with Continue to make sure Repeating happens")]
    private readonly UpdatingPrimaryValue RepeatNameConstant = new();
    private string? CurrentRepeatName = null;
    private bool IsDisposing;
    [EventOccasion("The job to maintain")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Dynamic source of timespan string")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when the repeat name was not set.")]
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
            Timers.Clear();
            StartMaintaining(new RepeatInteraction(this.CurrentRepeatName, interaction), timespanResult);
        }
    }

    private void StartMaintaining(RepeatInteraction interaction, TimeSpan interval)
    {
        int time = (int)(Math.Min(int.MaxValue, interval.TotalMilliseconds));
        Thread? nt = null;
        nt = new Thread(_ =>
        {
            while (interaction.IsRunning && Timers.Contains(nt!))
            {
                interaction.IsRunning = false;
                OnThen?.Invoke(this, interaction);
                Thread.Sleep(time);
            }
        });
        Timers.Add(nt);
        nt.Start();
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        if (!IsDisposing)
        {
            IsDisposing = true;
            Timers.Clear();
        }
    }
}
