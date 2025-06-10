#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;

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
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}