#nullable enable
using System.Security.Cryptography;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Profiler;
#pragma warning disable 67
public class DisableProfiler : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => ServiceProfiler.Instance.Disable();
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}