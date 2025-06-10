#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
public class StopWebServer : IService
{
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ServerCommandInteraction(interaction, ServerCommand.Stop));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
