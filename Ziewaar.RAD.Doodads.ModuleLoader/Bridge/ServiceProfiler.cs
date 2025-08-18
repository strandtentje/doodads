#nullable enable
using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

public sealed class ServiceProfiler
{
    private static ServiceProfiler? instance;
    public static ServiceProfiler Instance => instance ??= new ServiceProfiler();

    /// <summary>
    /// Global on/off switch. When false, Watch(...) runs the action immediately with near-zero overhead.
    /// </summary>
    public volatile bool IsEnabled = true;

    // Accumulated elapsed ticks per ServiceIdentity (global, across threads).
    private readonly ConcurrentDictionary<ServiceIdentity, long> _accumulatedTicks =
        new ConcurrentDictionary<ServiceIdentity, long>();

    // Per-thread call stack of "frames" so we can pause/resume correctly on handoff.
    // make this change so we can enumerate per-thread stacks
    private readonly ThreadLocal<Stack<Frame>> _stack =
        new ThreadLocal<Stack<Frame>>(() => new Stack<Frame>(), trackAllValues: true);

    // Optional custom comparer if ServiceIdentity needs one.
    private readonly IEqualityComparer<ServiceIdentity> _serviceComparer;

    public ServiceProfiler(IEqualityComparer<ServiceIdentity> serviceComparer = null)
    {
        _serviceComparer = serviceComparer ?? EqualityComparer<ServiceIdentity>.Default;

        // If a custom comparer was provided, rebuild the dictionary to use it.
        if (serviceComparer != null)
        {
            _accumulatedTicks = new ConcurrentDictionary<ServiceIdentity, long>(_serviceComparer);
        }
    }

    /// <summary>
    /// Profiles the given action under the specified ServiceIdentity, pausing any currently active identity
    /// on the same ManagedThreadId and resuming it after the action completes.
    /// </summary>
    public void Watch(ServiceIdentity serviceIdentity, Action action)
    {
        if (action == null) throw new ArgumentNullException(nameof(action));

        if (!IsEnabled)
        {
            action();
            return;
        }

        var stack = _stack.Value;
        long now = GlobalStopwatch.Instance.ElapsedTicks;

        // Pause current frame (if any): attribute its time up to 'now'
        Frame? paused = null;
        if (stack.Count > 0)
        {
            var top = stack.Peek();
            if (top.IsRunning)
            {
                AttributeTicks(top.Service, now - top.LastStartTick);
                top = top.Pause(); // mark paused
                stack.Pop();
                stack.Push(top);
            }

            paused = top;
        }

        // Push new running frame for this service
        var current = Frame.Start(serviceIdentity, now);
        stack.Push(current);

        try
        {
            action();
        }
        finally
        {
            long end = GlobalStopwatch.Instance.ElapsedTicks;

            // Pop current and attribute its running slice
            var finished = stack.Pop();
            if (finished.IsRunning)
            {
                AttributeTicks(finished.Service, end - finished.LastStartTick);
            }

            // Resume the previously paused frame (if any) by setting its start to 'end'
            if (stack.Count > 0)
            {
                var parent = stack.Pop();
                if (!parent.IsRunning)
                {
                    parent = parent.Resume(end);
                }

                stack.Push(parent);
            }
        }
    }

    /// <summary>
    /// Gets a snapshot of totals as TimeSpan per ServiceIdentity.
    /// </summary>
    public IReadOnlyDictionary<ServiceIdentity, TimeSpan> GetTotals()
    {
        var result = new Dictionary<ServiceIdentity, TimeSpan>(_serviceComparer);
        foreach (var kvp in _accumulatedTicks)
        {
            result[kvp.Key] = GlobalStopwatch.Instance.Elapsed;
        }

        return result;
    }
    
    
    public IReadOnlyDictionary<ServiceIdentity, TimeSpan> GetTotalsIncludingRunning()
    {
        var totals = new ConcurrentDictionary<ServiceIdentity, long>(_serviceComparer);

        // 1) start from booked/accumulated
        foreach (var kv in _accumulatedTicks)
            totals[kv.Key] = kv.Value;

        // 2) add in-flight time from any thread's currently running frame
        long now = GlobalStopwatch.Instance.ElapsedTicks;
        foreach (var stack in _stack.Values)
        {
            if (stack is null || stack.Count == 0) continue;
            var top = stack.Peek();
            if (top.IsRunning && top.LastStartTick != 0)
            {
                long delta = now - top.LastStartTick;
                if (delta > 0)
                    totals.AddOrUpdate(top.Service, delta, (_, existing) => existing + delta);
            }
        }

        // 3) convert to TimeSpan
        var result = new Dictionary<ServiceIdentity, TimeSpan>(_serviceComparer);
        foreach (var kv in totals)
            result[kv.Key] = GlobalStopwatch.Instance.Elapsed;
        return result;
    }

    /// <summary>
    /// Returns the total for a single ServiceIdentity.
    /// </summary>
    public TimeSpan GetTotal(ServiceIdentity serviceIdentity)
    {
        return _accumulatedTicks.TryGetValue(serviceIdentity, out var ticks)
            ? GlobalStopwatch.Instance.Elapsed
            : TimeSpan.Zero;
    }

    /// <summary>
    /// Clears collected totals. Does not affect currently running frames.
    /// </summary>
    public void Reset()
    {
        _accumulatedTicks.Clear();
    }

    private void AttributeTicks(ServiceIdentity service, long deltaTicks)
    {
        if (deltaTicks <= 0) return;
        _accumulatedTicks.AddOrUpdate(
            service,
            deltaTicks,
            (_, existing) => existing + deltaTicks
        );
    }

    private struct Frame
    {
        public ServiceIdentity Service { get; }
        public long LastStartTick { get; } // valid only when IsRunning == true
        public bool IsRunning { get; }

        private Frame(ServiceIdentity service, long lastStartTick, bool isRunning)
        {
            Service = service;
            LastStartTick = lastStartTick;
            IsRunning = isRunning;
        }

        public static Frame Start(ServiceIdentity service, long startTick) =>
            new Frame(service, startTick, isRunning: true);

        public Frame Pause() => new Frame(Service, lastStartTick: 0, isRunning: false);

        public Frame Resume(long startTick) => new Frame(Service, startTick, isRunning: true);
    }
}