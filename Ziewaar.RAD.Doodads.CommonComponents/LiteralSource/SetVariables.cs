namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

public class SetVariables : IService
{
    [NamedBranch] public event EventHandler<IInteraction> Continue;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction) => 
        Continue?.Invoke(this, new VariablesInteraction(interaction, serviceConstants));
}