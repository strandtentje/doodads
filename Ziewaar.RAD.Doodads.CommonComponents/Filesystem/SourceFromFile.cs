#nullable enable
using System.Text;
using Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;

[Category("System & IO")]
[Title("Make source stream from file")]
[Description("""Provided a filename, make its (binary) contents the source stream""")]
public class SourceFromFile : IService
{
    [NamedSetting("binary", "Set this to true if we're sourcing a binary stream")]
    private readonly UpdatingKeyValue IsBinaryConst = new UpdatingKeyValue("binary");
    private bool IsCurrentlyBinary = true;
    [EventOccasion("Sink file path here")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sourcing file contents here")]
    public event CallForInteraction? OnElse;
    [EventOccasion("File didn't exist.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, IsBinaryConst).IsRereadRequired(() => true, out bool isBinary))
            this.IsCurrentlyBinary = isBinary;

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var candidatePath = tsi.ReadAllText();
        var fileInfo = new FileInfo(candidatePath);
        if (!fileInfo.Exists)
            OnException?.Invoke(this, interaction.AppendRegister("file didn't exist."));
        else if (this.IsCurrentlyBinary)
            using (var stream = fileInfo.OpenRead())
                OnElse?.Invoke(this, new BinarySourcingInteraction(interaction, stream, MimeMapping.GetMimeInfo(fileInfo)));
        else
        {
            Encoding encoding;
            using (var reader = new StreamReader(fileInfo.OpenRead()))
                encoding = reader.CurrentEncoding;
            using (var stream = fileInfo.OpenRead())
                OnElse?.Invoke(this, new TextSourcingInteraction(interaction, stream, encoding, MimeMapping.GetMimeInfo(fileInfo)));
        }

    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
