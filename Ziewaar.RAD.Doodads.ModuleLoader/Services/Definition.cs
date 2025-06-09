using Ziewaar.RAD.Doodads.CoreLibrary.Attributes;

namespace Ziewaar.RAD.Doodads.ModuleLoader.Services;
public class Definition : IService
{    
    public event EventHandler<IInteraction> OnError;
    [DefaultBranch]
    public event EventHandler<IInteraction> Begin;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        if (interaction.TryGetClosest<CallingInteraction>(out var ci))
        {
            Begin?.Invoke(this, new VariablesInteraction(interaction, serviceConstants));
        } else if (interaction.TryGetClosest<ISelfStartingInteraction>(out var ss))
        {
            Begin?.Invoke(this, new VariablesInteraction(interaction, serviceConstants));
        }
    }
}
