namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
public interface IService
{
    public event CallForInteraction OnThen;
    public event CallForInteraction OnElse;
    public event CallForInteraction OnException; 
    void Enter(
        StampedMap constants,
        IInteraction interaction);
    void HandleFatal(IInteraction source, Exception ex);
}
