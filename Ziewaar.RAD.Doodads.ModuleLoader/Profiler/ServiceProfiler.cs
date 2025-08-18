#nullable enable
using System.Collections.Concurrent;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
using ProfilingFrameStackByThread =
    System.Threading.ThreadLocal<System.Collections.Generic.Stack<
        Ziewaar.RAD.Doodads.ModuleLoader.Profiler.ProfilingScopedServiceFrame>>;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;

public sealed class ServiceProfiler
{
    public static ServiceProfiler Instance { get; private set; } = new();
    public static void ResetInstance() => Instance = new();
    private readonly ConcurrentDictionary<ServiceIdentity, TimeSpan> FinishedServiceTotals = new();
    private readonly ProfilingFrameStackByThread StacksByThread = new(() => new(), trackAllValues: true);
    private (Action<ServiceIdentity> Push, Action Pop) Scope = (PushScopeDisabled, PopScopeDisabled);
    public void Enable() => Scope = (PushScopeEnabled, PopScopeEnabled);
    public void Disable() => Scope = (PushScopeDisabled, PopScopeDisabled);
    public void Watch(ServiceIdentity serviceIdentity, Action operationToMonitor) => ForScope(scope =>
    {
        if (operationToMonitor == null) throw new ArgumentNullException(nameof(operationToMonitor));
        scope.Push(serviceIdentity);
        try
        {
            operationToMonitor();
        }
        finally
        {
            scope.Pop();
        }
    });
    private void PushScopeEnabled(ServiceIdentity nextService) => ForThreadInstant((risingTime, threadStack) =>
    {
        if (threadStack.Any() && threadStack.Peek() is { } caller &&
            caller.TryPause(risingTime, out var runtime))
            FinishedServiceTotals.AddOrUpdate(nextService, runtime, (_, existing) => existing + runtime);
        threadStack.Push(new ProfilingScopedServiceFrame(nextService, risingTime));
    });
    private void PopScopeEnabled() => ForThreadInstant((fallingTime, threadStack) =>
    {
        var afterExecution = threadStack.Pop();
        var frameTime = fallingTime - afterExecution.RuntimeLastTouched;
        if (afterExecution.IsRunning)
            FinishedServiceTotals.AddOrUpdate(afterExecution.Service, frameTime, (_, existing) => existing + frameTime);
        else
            GlobalLog.Instance?.Warning(
                "Frame was already stopped, so runtime was not registered {serviceInfo}",
                afterExecution.Service);
        if (threadStack.Count > 0)
            threadStack.Peek().TryResume(fallingTime);
    });
    private static void PushScopeDisabled(ServiceIdentity x) { }
    private static void PopScopeDisabled() { }

    public IOrderedEnumerable<(ServiceIdentity service, TimeSpan time, int totalCount, int activeCount)>
        GetHeaviestRunningTotals() =>
        ForInstant(currentRuntime => StacksByThread.Values
            .Where(threadStack => threadStack?.Count > 0)
            .Select(threadStack => threadStack.Peek())
            .Where(scopeFrame => scopeFrame.IsRunning && scopeFrame.RuntimeLastTouched != TimeSpan.Zero)
            .Select(x => (service: x.Service, time: currentRuntime - x.RuntimeLastTouched, active: true))
            .Concat(FinishedServiceTotals.Select(x => (service: x.Key, time: x.Value, active: false)))
            .OrderByDescending(x => x.time)
            .GroupBy(anyTotal => anyTotal.service)
            .Select(totalGroup => (
                service: totalGroup.Key,
                time: totalGroup.Aggregate(TimeSpan.Zero, (sum, anyTotal) => sum + anyTotal.time, result => result),
                count: totalGroup.Count(),
                active: totalGroup.Count(anyTotal => anyTotal.active)))
            .OrderByDescending(x => x.time));
    private TReturn ForInstant<TReturn>(Func<TimeSpan, TReturn> action) => action(GlobalStopwatch.Instance.Elapsed);
    private void ForThreadInstant(Action<TimeSpan, Stack<ProfilingScopedServiceFrame>> action) =>
        action(GlobalStopwatch.Instance.Elapsed, StacksByThread.Value);
    private void ForScope(Action<(Action<ServiceIdentity> Push, Action Pop)> action) => action(Scope);
}