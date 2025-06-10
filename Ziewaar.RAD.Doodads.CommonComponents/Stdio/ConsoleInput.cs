#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class ConsoleInput : IService
{
    private static readonly Stream StandardInput = System.Console.OpenStandardInput();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ConsoleSourceInteraction(interaction, StandardInput));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
