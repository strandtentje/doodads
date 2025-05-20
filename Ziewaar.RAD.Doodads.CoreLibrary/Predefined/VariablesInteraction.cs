using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VariablesInteraction(IInteraction parent, SortedList<string, object> variables) : IInteraction
{
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables => variables;
    public static VariablesInteraction ForError(
        IInteraction parent, string message) => new VariablesInteraction(
            parent, new SortedList<string, object> { { "error", message } });

    public static VariablesInteractionBuilder CreateBuilder(IInteraction parent) => new VariablesInteractionBuilder(parent);
}


