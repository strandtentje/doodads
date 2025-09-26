#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
#pragma warning disable 67

[Category("Diagnostics & Debug")]
[Title("Resume dumps globally")]
[Description("")]
public class EnableDump : IService
{
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => DumpSwitch.IsEnabled = true;
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
