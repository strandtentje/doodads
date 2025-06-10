namespace Ziewaar.RAD.Doodads.ModuleLoader;
public interface IAmbiguousServiceWrapper
{
    void OnThen(CallForInteraction dlg);
    void OnElse(CallForInteraction dlg);
    void OnDone(CallForInteraction dlg);
    void Cleanup();
    void Run(object sender, IInteraction interaction);
}