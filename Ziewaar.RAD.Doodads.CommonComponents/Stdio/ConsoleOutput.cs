#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class ConsoleOutput : IService
{
    public static Stream StandardOutput = Console.OpenStandardOutput();
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new ConsoleSinkInteraction(interaction, StandardOutput));
}
