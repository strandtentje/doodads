namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions
{
    public interface IControlCommandInteraction : IInteraction
    {
        object Command { get; }
        bool CanApply(object command);
    }
}