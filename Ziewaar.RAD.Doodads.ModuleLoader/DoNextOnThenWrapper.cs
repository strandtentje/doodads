#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class DoNextOnThenWrapper : IAmbiguousServiceWrapper
{
    private CallForInteraction? ElseDelegate, DoneDelegate;
    private IAmbiguousServiceWrapper[]? ServiceSequence;
    public void OnThen(CallForInteraction dlg)=>
        throw new InvalidOperationException("Cannot do then/else on alternative services");
    public void OnElse(CallForInteraction dlg)
    {
        this.ElseDelegate = dlg;
    }
    public void OnDone(CallForInteraction dlg)
    {
        this.DoneDelegate = dlg;
    }
    public void Cleanup()
    {
        this.DoneDelegate = null;
        this.ElseDelegate = null;
        if (ServiceSequence == null) 
            return;
        foreach (var ambiguousServiceWrapper in ServiceSequence)
            ambiguousServiceWrapper.Cleanup();
        ServiceSequence = null;
    }
    public void Run(object sender, IInteraction interaction)
    {
        this.ServiceSequence!.ElementAt(0).Run(sender, interaction);
        this.DoneDelegate?.DynamicInvoke(this, interaction);
    }
    public IEnumerable<(DefinedServiceWrapper wrapper, IService service)> GetAllServices() => ServiceSequence?.SelectMany(x => x.GetAllServices()) ?? [];
    private void HandleElse(object sender, IInteraction interaction)
    {
        this.ElseDelegate?.DynamicInvoke(sender, interaction);
    }
    public void SetTarget(ServiceBuilder[] toArray)
    {
        this.ServiceSequence = toArray.Select(x => x.CurrentService!).ToArray();
        for (var i = 0; i < toArray.Length; i++)
            toArray[i].CurrentService!.OnElse(HandleElse);
        for (var i = 0; i < toArray.Length - 1; i++)
            toArray[i].CurrentService!.OnThen(toArray[i + 1].Run);
    }
}