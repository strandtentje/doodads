namespace Ziewaar.RAD.Doodads.ModuleLoader.Interactions;
public class CallingInteraction(IInteraction offset, Action<IInteraction> continued) : IInteraction
{
    public IInteraction Parent => offset;
    public IReadOnlyDictionary<string, object> Variables => offset.Variables;
    public void Continue(IInteraction interaction) => continued(interaction);
}
