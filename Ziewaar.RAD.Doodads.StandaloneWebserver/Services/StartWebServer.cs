#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
public class StartWebServer : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ServerCommandInteraction(interaction, ServerCommand.Start));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
