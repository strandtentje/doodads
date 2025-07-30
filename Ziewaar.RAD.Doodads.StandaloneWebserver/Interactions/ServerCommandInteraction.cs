namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class ServerCommandInteraction(IInteraction parent, ServerCommand command) : IControlCommandInteraction
{
    public object Command { get; private set; } = command;
    public bool CanApply(object command) => command is ServerCommand;
    public void Consume() => Command = ServerCommand.None;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}