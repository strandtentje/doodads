#pragma warning disable 67
#nullable enable
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.InteropServices;
using Ziewaar.Common.Aardvargs;

namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
[Category("System & IO")]
[Title("Open input stream")]
[Description("Provides a source for reading lines from the console")]
public class ConsoleInput : IService
{
    private static readonly Stream StandardInput = System.Console.OpenStandardInput();
    [EventOccasion("Console stream is ready to source")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ConsoleSourceInteraction(interaction, StandardInput));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
