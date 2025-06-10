#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;



public class StartLineReader : IService
{
    private readonly UpdatingPrimaryValue LineReaderNameConstant = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, LineReaderNameConstant).IsRereadRequired(out string? lineReaderName);
        if (lineReaderName == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "line reader name required"));
            return;
        }
        if (interaction.TryGetClosest<ISourcingInteraction>(out var sourcing) && 
            sourcing != null)
        {
            var reader = new StreamReader(sourcing.SourceBuffer, sourcing.TextEncoding,
                detectEncodingFromByteOrderMarks: false, bufferSize: 2048, leaveOpen: true);
            var linesInteraction = new ReadLinesInteraction(interaction, lineReaderName, reader);
            linesInteraction.EndOfStream += (s, e) =>
            {
                OnElse?.Invoke(this, interaction);
            };
            OnThen?.Invoke(this, linesInteraction);
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
