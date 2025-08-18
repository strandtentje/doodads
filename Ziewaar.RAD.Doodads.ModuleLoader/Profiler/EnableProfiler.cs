namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;
#nullable enable
#pragma warning disable 67
public class EnableProfiler : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => ServiceProfiler.Instance.Enable();
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}