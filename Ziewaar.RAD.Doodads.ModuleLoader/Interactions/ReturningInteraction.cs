namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
public class ReturningInteraction(IInteraction parent, CallingInteraction cause, IReadOnlyDictionary<string, object> variables)
: IInteraction
{
    public CallingInteraction Cause => cause;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => variables;
}
