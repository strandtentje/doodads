#nullable enable
using Ziewaar.RAD.Doodads.ModuleLoader.Bridge;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;

public class ProfilingScopedServiceFrame(ServiceIdentity service, TimeSpan startTime)
{
    public ServiceIdentity Service => service;
    public TimeSpan RuntimeLastTouched { get; private set; } = startTime;
    public bool IsRunning { get; private set; } = true;
    public bool TryPause(TimeSpan pauseTime, out TimeSpan runtime)
    {
        if (!IsRunning) return false;
        runtime = pauseTime - RuntimeLastTouched;
        IsRunning = false;
        return true;
    }
    public bool TryResume(TimeSpan start)
    {
        if (IsRunning) return false;
        IsRunning = true;
        RuntimeLastTouched = start;
        return true;
    }
}