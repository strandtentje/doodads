#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Stop dumps in this context")]
[Description("")]
public class StopDump : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new DumpStopper(interaction));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
