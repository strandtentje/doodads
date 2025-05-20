namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VariablesInteractionBuilder(IInteraction parent)
{
    private readonly SortedList<string, object> _state = new();
    public VariablesInteractionBuilder Add(string key, object value)
    {
        _state.Add(key, value);
        return this;
    }
    public VariablesInteraction Build() => new VariablesInteraction(parent, _state);
}


