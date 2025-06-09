namespace Ziewaar.RAD.Doodads.ModuleLoader;
public interface IAmbiguousServiceWrapper
{
    void OnThen(Delegate dlg);
    void OnElse(Delegate dlg);
    void OnDone(Delegate dlg);
    void Cleanup();
    void Run(IInteraction interaction);
}