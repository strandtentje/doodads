#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
#pragma warning disable 67

[Category("Diagnostics & Debug")]
[Title("Stop dumps in this context")]
[Description("")]
public class StopDump : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new DumpStopper(interaction));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
