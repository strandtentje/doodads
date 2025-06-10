namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
public interface IService
{
    public event EventHandler<IInteraction> OnThen;
    public event EventHandler<IInteraction> OnElse;
    public event EventHandler<IInteraction> OnException; 
    void Enter(
        StampedMap constants,
        IInteraction interaction);
    void HandleFatal(IInteraction source, Exception ex);
}
