#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("Diagnostics & Debug")]
[Title("Resume dumps globally")]
[Description("")]
public class EnableDump : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => DumpSwitch.IsEnabled = true;
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
