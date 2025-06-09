namespace Ziewaar.RAD.Doodads.ModuleLoader;
public class DoNextOnThenWrapper : IAmbiguousServiceWrapper
{
    private Delegate? ElseDelegate, DoneDelegate;
    private IAmbiguousServiceWrapper[]? ServiceSequence;
    public void OnThen(Delegate dlg)=>
        throw new InvalidOperationException("Cannot do then/else on alternative services");
    public void OnElse(Delegate dlg)
    {
        if (this.ElseDelegate != null)
            throw new InvalidOperationException("Cannot handle done twice");
        this.ElseDelegate = dlg;
    }
    public void OnDone(Delegate dlg)
    {
        if (this.DoneDelegate != null)
            throw new InvalidOperationException("Cannot handle done twice");
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
    public void Run(IInteraction interaction)
    {
        this.ServiceSequence!.ElementAt(0).Run(interaction);
        this.DoneDelegate?.DynamicInvoke(this, interaction);
    }
    public void SetTarget(ServiceBuilder[] toArray)
    {
        this.ServiceSequence = toArray.Select(x => x.CurrentService!).ToArray();
        var handleElse = new Action<object, IInteraction>((s, e) =>
        {
            this.ElseDelegate?.DynamicInvoke(s, e);
        });
        for (var i = 0; i < toArray.Length - 1; i++)
        {
            toArray[i].CurrentService!.OnThen(toArray[i + 1].Run);
            toArray[i].CurrentService!.OnElse(handleElse);
        }
    }
}