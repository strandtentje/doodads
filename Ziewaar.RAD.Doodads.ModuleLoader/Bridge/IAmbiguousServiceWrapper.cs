#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
public interface IAmbiguousServiceWrapper
{
    void OnThen(CallForInteraction dlg);
    void OnElse(CallForInteraction dlg);
    void OnDone(CallForInteraction dlg);
    void Cleanup();
    void Run(object sender, IInteraction interaction);
    IEnumerable<(DefinedServiceWrapper wrapper, IService service)> GetAllServices();
}