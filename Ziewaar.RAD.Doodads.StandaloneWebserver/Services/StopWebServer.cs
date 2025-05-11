namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class StopWebServer : IService
{
    public event EventHandler<IInteraction>? OnError;
    public event EventHandler<IInteraction>? ToStop;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction) =>
        ToStop?.Invoke(this, new ServerCommandInteraction(interaction, ServerCommand.Stop));
}
