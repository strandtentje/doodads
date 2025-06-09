namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
public class ReturningInteraction(IInteraction parent, CallingInteraction cause, SortedList<string, object> variables)
    : VariablesInteraction(parent, variables)
{
    public CallingInteraction Cause => cause;
}
