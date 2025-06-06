
namespace Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

public class VoidService : IService
{
    public event EventHandler<IInteraction> OnError;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction)
    {
        OnError?.Invoke(this, interaction);
    }
}
