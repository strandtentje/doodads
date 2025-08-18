#nullable enable
using System.Security.Cryptography;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;
#pragma warning disable 67
[Category("Diagnostics & Debug")]
[Title("Disengage the profiler")]
[Description("Use in conjunction with profiler services like ProfileByHeaviest")]
public class DisableProfiler : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => ServiceProfiler.Instance.Disable();
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}