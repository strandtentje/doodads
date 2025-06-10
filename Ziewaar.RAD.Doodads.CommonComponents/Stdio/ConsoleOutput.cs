#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class ConsoleOutput : IService
{
    public static Stream StandardOutput = Console.OpenStandardOutput();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new ConsoleSinkInteraction(interaction, StandardOutput));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
