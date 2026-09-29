using Ejije.Logging;
using TagLib;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using File = System.IO.File;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class AtomicMetadata : BasicService
{
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;

    private static readonly Log UnsupportedFile =
        Log.Oops("Received unsupported {file} for metadata reading; {exception}");

    private readonly SortedList<string, (Tag tag, DateTime age)> Cache = new();

    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var candidateFile = Register(interaction);
        if (!File.Exists(candidateFile))
            throw new BasicException($"File not found for metadata reading; {candidateFile}");
        try
        {
            if (!constants.NamedItems.TryGetValue("cache", out var isCacheOn) || !Convert.ToBoolean(isCacheOn))
            {
                using var file = TagLib.File.Create(candidateFile);
                OnThen?.Invoke(this, new FileMetadataInteraction(interaction, candidateFile, file.Tag));
            }
            else
            {
                (Tag tag, DateTime age) item;
                lock (Cache)
                {
                    if (!Cache.TryGetValue(candidateFile, out item))
                    {
                        using var file = TagLib.File.Create(candidateFile);
                        item = Cache[candidateFile] = (file.Tag, DateTime.Now);
                    }

                    if (Cache.Count > 4096)
                        Cache.Clear();
                }
                OnThen?.Invoke(this, new FileMetadataInteraction(interaction, candidateFile, item.tag));
            }
        }
        catch (Exception ex)
        {
            Log.Post(UnsupportedFile, candidateFile, ex);
            OnElse?.Invoke(this, interaction);
        }
    }
}