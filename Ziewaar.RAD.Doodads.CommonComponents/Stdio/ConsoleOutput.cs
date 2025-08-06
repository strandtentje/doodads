#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

[Category("System & IO")]
[Title("Sink to Console")]
[Description("""
             Scopes a sink to write text to the console.
             """)]
public class ConsoleOutput : IService
{
    public static Stream StandardOutput = Console.OpenStandardOutput();
    [EventOccasion("Sink console text here")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) => OnThen?.Invoke(this, new ConsoleSinkInteraction(interaction, StandardOutput));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
