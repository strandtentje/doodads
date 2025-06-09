namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
public interface IService
{
    public event EventHandler<IInteraction> OnThen;
    public event EventHandler<IInteraction> OnElse;
    void Enter(
        ServiceConstants serviceConstants,
        IInteraction interaction);
}
