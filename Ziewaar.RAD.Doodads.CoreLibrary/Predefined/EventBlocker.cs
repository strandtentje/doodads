#nullable enable
using System.Collections.Concurrent;
using System.Threading;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public abstract class EventBlocker<TArgs> : IDisposable
{
    public EventBlocker() => BindEvent();
    private readonly EventWaitHandle Blocker = new(false, EventResetMode.AutoReset);
    private readonly ConcurrentQueue<TArgs> argQueue = new ConcurrentQueue<TArgs>();
    private readonly object LockObject = new();
    public bool IsRunning { get; private set; } = true;
    protected abstract void BindEvent();
    protected void Handler(object? sender, TArgs args)
    {
        if (!IsRunning) return;
        lock (LockObject)
        {
            argQueue.Enqueue(args);
        }
        Blocker.Set();
    }
    public bool TryTake(out TArgs? args)
    {
        args = default;
        try
        {
            Blocker.WaitOne();
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        if (!IsRunning) return false;
        lock (LockObject)
        {
            argQueue.TryDequeue(out args);
        }
        return true;
    }
    protected abstract void UnbindEvent();
    public void Dispose()
    {
        UnbindEvent();
        IsRunning = false;
        Blocker.Set();
        Blocker.Dispose();
    }
}