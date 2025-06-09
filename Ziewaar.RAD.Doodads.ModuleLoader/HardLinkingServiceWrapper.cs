namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class HardLinkingServiceWrapper : IAmbiguousServiceWrapper
{
    private IAmbiguousServiceWrapper? Target;
    public void OnThen(Delegate dlg) => Target!.OnThen(dlg);
    public void OnElse(Delegate dlg) => Target!.OnElse(dlg);
    public void OnDone(Delegate dlg) => Target!.OnDone(dlg);
    public void Cleanup()
    {
        this.Target = null;
    }
    public void Run(IInteraction interaction) => this.Target!.Run(interaction);
    public void SetTarget(ServiceBuilder target) => this.Target = target.CurrentService!;
}