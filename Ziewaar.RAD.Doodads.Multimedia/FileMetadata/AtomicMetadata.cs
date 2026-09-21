using Ejije.Logging;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class AtomicMetadata : BasicService
{
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;

    private static readonly Log UnsupportedFile =
        Log.Oops("Received unsupported {file} for metadata reading; {exception}");

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var candidateFile = Register(interaction);
        if (!File.Exists(candidateFile))
            throw new BasicException($"File not found for metadata reading; {candidateFile}");
        try
        {
            using var file = TagLib.File.Create(candidateFile);
            OnThen?.Invoke(this, new FileMetadataInteraction(interaction, candidateFile, file.Tag));
        }
        catch (Exception ex)
        {
            Log.Post(UnsupportedFile, candidateFile, ex);
            OnElse?.Invoke(this, interaction);
        }
    }
}