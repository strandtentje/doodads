namespace Ziewaar.RAD.Doodads.CommonComponents.Module;
public class ReturningInteraction(IInteraction parent, CallingInteraction cause, SortedList<string, object> variables)
    : VariablesInteraction(parent, variables)
{
    public CallingInteraction Cause => cause;
}
