using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;

#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
[Title("Open input text stream for reading its lines")]
[Description("""
             Useful for example with console or CSV; takes the lines of the file
             and exposes them as a list, without loading them into memory directly.
             """)]
public class Open : IService
{
    [PrimarySetting("Name to use for this line reader; must be the same for the Close")]
    private readonly UpdatingPrimaryValue LineReaderNameConstant = new();
    [EventOccasion("List of lines comes out here. Useful in conjunction with ie. Pop.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Happens when no more lines were left to read.")]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens because no name was set.")]
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
