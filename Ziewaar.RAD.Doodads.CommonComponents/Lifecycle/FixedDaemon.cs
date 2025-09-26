#nullable enable
using System.Threading;
using Ziewaar.RAD.Doodads.CoreLibrary.IterationSupport;

namespace Ziewaar.RAD.Doodads.CommonComponents.Lifecycle;
public class FixedDaemon : IService, IDisposable
{
    private static readonly SortedList<string, FixedDaemon> NamedDaemons = new();
    public static IReadOnlyDictionary<string, FixedDaemon> Daemons => NamedDaemons;
    private readonly UpdatingPrimaryValue DaemonNameConstant = new();
    private string? CurrentDaemonName;
    private EventWaitHandle StopSignal = new(false, EventResetMode.ManualReset);
    private bool IsStopCalled = false;
    public void Stop()
    {
        IsStopCalled = true;
        StopSignal.Set();
    }
    public void Restart()
    {
        IsStopCalled = false;
        StopSignal.Set();
    }
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (CurrentDaemonName != null && !string.IsNullOrWhiteSpace(CurrentDaemonName))
            NamedDaemons.Remove(CurrentDaemonName);
        if ((constants, DaemonNameConstant).IsRereadRequired(out string? daemonNameCandidate))
            CurrentDaemonName = daemonNameCandidate;
        if (CurrentDaemonName == null || string.IsNullOrWhiteSpace(CurrentDaemonName))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "daemon name required"));
            return;
        }
        NamedDaemons[CurrentDaemonName] = this;
        if (!interaction.TryGetClosest<RepeatInteraction>(out var repeater, x => x.RepeatName == CurrentDaemonName)
            || repeater == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "originator not daemonizable"));
            return;
        }

        try
        {
            IsStopCalled = true;
            StopSignal.Set();
            IsStopCalled = false;
            OnThen?.Invoke(this, interaction);
        }
        finally
        {
            try
            {
                StopSignal.Reset();
                StopSignal.WaitOne();
                repeater.IsRunning = !IsStopCalled;
            }
            catch (ObjectDisposedException)
            {
                repeater.IsRunning = false;
            }
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose()
    {
        if (CurrentDaemonName != null)
            NamedDaemons.Remove(CurrentDaemonName);
        IsStopCalled = true;
        try
        {
            StopSignal.Set();
            StopSignal.Dispose();
        }
        catch (ObjectDisposedException)
        {
        }
    }
}