#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class SoftLinkingServiceWrapper : IAmbiguousServiceWrapper
{
    private IAmbiguousServiceWrapper? Target;
    private Delegate? DoneDelegate;
    public void OnThen(Delegate dlg) =>
        throw new InvalidOperationException("Cannot do then/else on referenced services");
    public void OnElse(Delegate dlg) =>
        throw new InvalidOperationException("Cannot do then/else on referenced services");
    public void OnDone(Delegate dlg)
    {
        if (this.DoneDelegate != null)
            throw new ArgumentException("Done cant be set twice", nameof(dlg));
        this.DoneDelegate = dlg;
    }
    public void Cleanup()
    {
        this.DoneDelegate = null;
        this.Target = null;
    }
    public void Run(IInteraction interaction)
    {
        this.Target!.Run(interaction);
        this.DoneDelegate?.DynamicInvoke(this, interaction);
    }
    public void SetTarget(ServiceBuilder target) => this.Target = target.CurrentService!;
}