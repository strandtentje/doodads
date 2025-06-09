#nullable enable
using Ziewaar.RAD.Doodads.CommonComponents.Stdio;
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class ConsoleInput : IService
{
    private static readonly Stream StandardInput = System.Console.OpenStandardInput();
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        OnThen?.Invoke(this, new ConsoleSourceInteraction(interaction, StandardInput));
    }
}

public class IterateText : IService
{
    public event EventHandler<IInteraction>? OnThen;
    public event EventHandler<IInteraction>? OnElse;
    public event EventHandler<IInteraction>? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.TryGetClosest<ISourcingInteraction>(out var sourcing) && 
            sourcing != null)
        {
            using var x = new StreamReader(sourcing.SourceBuffer, sourcing.TextEncoding,
                detectEncodingFromByteOrderMarks: false, bufferSize: 2048, leaveOpen: true);
            IEnumerable<string> Enumerate()
            {
                while (!x.EndOfStream && x.ReadLine() is { } line)
                    yield return line;
            }
            OnThen?.Invoke(this, new CommonInteraction(interaction, Enumerate()));
        }
    }
}