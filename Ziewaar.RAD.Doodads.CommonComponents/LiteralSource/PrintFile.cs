#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
public class PrintFile : IService
{
    private readonly UpdatingPrimaryValue ConstantFilename = new();
    private readonly UpdatingKeyValue PrintBinary = new("binary");
    private readonly UpdatingKeyValue SetContentLength = new("setlength");
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ConstantFilename).IsRereadRequired<object>(out var constantFilename);
        // if not set, we will let c# read the file in whatever encoding it has going on,
        // and then write it in the encoding the sink has going on.
        (constants, PrintBinary).IsRereadRequired<bool>(out var forceBinaryWriting);
        // we may be printing multiple files for concatenation, then we wouldnt wanna set content length
        (constants, SetContentLength).IsRereadRequired<bool>(out var setContentLength);

        var preferredFilename = constantFilename?.ToString() ?? interaction.Register as string;
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
            checkUpdateRequiredInteraction != null)
        {
            checkUpdateRequiredInteraction.IsRequired =
                checkUpdateRequiredInteraction.Original.LastSinkChangeTimestamp != fileInfo.LastWriteTime.Ticks;
        }
        else if (interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) &&
                   sinkingInteraction != null)
        {
            if (setContentLength == true)
                sinkingInteraction.SetContentLength64(fileInfo.Length);

            sinkingInteraction.LastSinkChangeTimestamp = fileInfo.LastWriteTime.Ticks;
            if (sinkingInteraction.TextEncoding is NoEncoding || forceBinaryWriting == true)
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
