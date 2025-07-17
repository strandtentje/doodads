namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
public class ReturningInteraction(IService returner, object? registerOverride, IInteraction parent, CallingInteraction cause, IReadOnlyDictionary<string, object> variables)
: IInteraction
{
    public IService Returner => returner;
    public CallingInteraction Cause => cause;
    public IInteraction Stack => parent;
    public object Register => registerOverride ?? parent.Register;
    public IReadOnlyDictionary<string, object> Memory => variables;
}
