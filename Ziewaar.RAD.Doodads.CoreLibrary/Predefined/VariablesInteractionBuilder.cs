namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VariablesInteractionBuilder(IInteraction parent)
{
    private readonly SortedList<string, object> State = new();
    public VariablesInteractionBuilder Add(string key, object value)
    {
        State.Add(key, value);
        return this;
    }
    public VariablesInteraction Build() => new VariablesInteraction(parent, State);
}


