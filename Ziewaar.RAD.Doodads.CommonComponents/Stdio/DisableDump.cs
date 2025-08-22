#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Stop dumps globally")]
[Description("")]
public class DisableDump : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => DumpSwitch.IsEnabled = false;
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
