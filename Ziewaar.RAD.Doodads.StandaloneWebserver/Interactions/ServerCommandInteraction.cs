namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class ServerCommandInteraction(IInteraction parent, ServerCommand command) : IInteraction
{
    public IInteraction Parent => parent;
    public IReadOnlyDictionary<string, object> Variables => parent.Variables;
    public ServerCommand Command { get; private set; } = command;
    public void Consume() => Command = ServerCommand.None;
}
