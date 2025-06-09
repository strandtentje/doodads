namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class DoNextOnElseWrapper : IAmbiguousServiceWrapper
{
    private Delegate? DoneDelegate;
    private IAmbiguousServiceWrapper[]? ServiceSequence;
    public void OnThen(Delegate dlg)=>
        throw new InvalidOperationException("Cannot do then/else on alternative services");
    public void OnElse(Delegate dlg)=>
        throw new InvalidOperationException("Cannot do then/else on alternative services");
    public void OnDone(Delegate dlg)
    {
        if (this.DoneDelegate != null)
            throw new InvalidOperationException("Cannot handle done twice");
        this.DoneDelegate = dlg;
    }
    public void Cleanup()
    {
        this.DoneDelegate = null;
        if (ServiceSequence == null) 
            return;
        foreach (var ambiguousServiceWrapper in ServiceSequence)
            ambiguousServiceWrapper.Cleanup();
        ServiceSequence = null;
    }
    public void Run(IInteraction interaction)
    {
        this.ServiceSequence!.ElementAt(0).Run(interaction);
        this.DoneDelegate?.DynamicInvoke(this, interaction);
    }
    public void SetTarget(ServiceBuilder[] toArray)
    {
        this.ServiceSequence = toArray.Select(x => x.CurrentService!).ToArray();
        for (var i = 0; i < toArray.Length - 1; i++)
            toArray[i].CurrentService!.OnElse(toArray[i + 1].Run);
    }
}