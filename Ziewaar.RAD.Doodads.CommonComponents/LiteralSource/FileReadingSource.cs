#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
public class FileReadingSource : IService
{
    private readonly UpdatingPrimaryValue ConstantFilename = new();
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ConstantFilename).IsRereadRequired<string>(out var constantFilename);

        var preferredFilename = constantFilename ?? interaction.Register as string;
        if (preferredFilename == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "either set a constant filename, or provide one thru the primary value"));
            return;
        }
        FileInfo fileInfo;
        try
        {
            fileInfo = new FileInfo(preferredFilename);
            if (!fileInfo.Exists)
            {
                using var x = fileInfo.CreateText();
                x.Write("");
                x.Flush();
            }
        }
        catch (Exception ex)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, ex.ToString()));
            return;
        }
        
        if (interaction.TryGetClosest<ICheckUpdateRequiredInteraction>(out var checkUpdateRequiredInteraction) && 
            checkUpdateRequiredInteraction != null &&
            checkUpdateRequiredInteraction.Original.LastSinkChangeTimestamp != fileInfo.LastWriteTime.Ticks
            )
        {
            checkUpdateRequiredInteraction.IsRequired = true;
        } else if (interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) && 
                   sinkingInteraction != null)
        {
            if (sinkingInteraction.TextEncoding is NoEncoding)
            {
                using var fileStream = fileInfo.OpenRead();
                fileStream.CopyTo(sinkingInteraction.SinkBuffer);
            }
            else
            {
                using var textStream = new StreamReader(preferredFilename, detectEncodingFromByteOrderMarks: true);
                sinkingInteraction.CopyTextFrom(textStream);
            }
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no sink found to write the file contents to"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}