#pragma warning disable 67
#nullable enable
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
[Category("Input from source")]
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
