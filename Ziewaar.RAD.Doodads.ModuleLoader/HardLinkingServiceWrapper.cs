#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class HardLinkingServiceWrapper : IAmbiguousServiceWrapper
{
    private IAmbiguousServiceWrapper? Target;
    public void OnThen(CallForInteraction dlg) => Target!.OnThen(dlg);
    public void OnElse(CallForInteraction dlg) => Target!.OnElse(dlg);
    public void OnDone(CallForInteraction dlg) => Target!.OnDone(dlg);
    public void Cleanup()
    {
        this.Target = null;
    }
    public void Run(object sender, IInteraction interaction) => this.Target!.Run(sender, interaction);
    public void SetTarget(ServiceBuilder target) => this.Target = target.CurrentService!;
}