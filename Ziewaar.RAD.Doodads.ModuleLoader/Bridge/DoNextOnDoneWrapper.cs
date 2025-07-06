#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader.Bridge;
public class DoNextOnDoneWrapper : IAmbiguousServiceWrapper
{
    private IAmbiguousServiceWrapper[]? ServiceSequence;
    public void OnThen(CallForInteraction dlg) =>
        throw new InvalidOperationException("Cannot do then/else/done on concatenated services");
    public void OnElse(CallForInteraction dlg) =>
        throw new InvalidOperationException("Cannot do then/else/done on concatenated services");
    public void OnDone(CallForInteraction dlg) =>
        throw new InvalidOperationException("Cannot do then/else/done on concatenated services");
    public void Cleanup()
    {
        if (ServiceSequence == null) 
            return;
        foreach (var ambiguousServiceWrapper in ServiceSequence)
            ambiguousServiceWrapper.Cleanup();
        ServiceSequence = null;
    }
    public void Run(object sender, IInteraction interaction) => this.ServiceSequence!.ElementAt(0).Run(sender, interaction);
    public IEnumerable<(DefinedServiceWrapper wrapper, IService service)> GetAllServices() => ServiceSequence?.SelectMany(x => x.GetAllServices()) ?? [];
    public void SetTarget(ServiceBuilder[] toArray)
    {
        this.ServiceSequence = toArray.Select(x => x.CurrentService!).ToArray();
        for (var i = 0; i < toArray.Length - 1; i++)
            toArray[i].CurrentService!.OnDone(toArray[i + 1].Run);
    }
}