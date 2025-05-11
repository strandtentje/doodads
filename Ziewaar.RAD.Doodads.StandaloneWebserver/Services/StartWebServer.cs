namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;

public class StartWebServer : IService
{
    public event EventHandler<IInteraction>? OnError;
    public event EventHandler<IInteraction>? ToStart;
    public void Enter(ServiceConstants serviceConstants, IInteraction interaction) =>
        ToStart?.Invoke(this, new ServerCommandInteraction(interaction, ServerCommand.Start));
}
