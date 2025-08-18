namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;
#nullable enable
#pragma warning disable 67
[Category("Diagnostics & Debug")]
[Title("Engage the profiler")]
[Description("Use in conjunction with profiler services like ProfileByHeaviest")]
public class EnableProfiler : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => ServiceProfiler.Instance.Enable();
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}