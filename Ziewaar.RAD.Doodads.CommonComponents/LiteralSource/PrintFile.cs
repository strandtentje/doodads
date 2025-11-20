#pragma warning disable 67
#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.LiteralSource;
[Category("Printing & Formatting")]
[Title("Works like Print, but uses the contents of a file instead")]
[Description("""
             Provide a file path as the primary setting to write its contents to the output. Unless specified otherwise,
             this will also take notice of the File encoding, and Output encoding. It will then translate if necessary
             to prevent weird characters.
             """)]
public class PrintFile : IService
{
    [PrimarySetting("File name to read data from")]
    private readonly UpdatingPrimaryValue ConstantFilename = new();
    [NamedSetting("binary", "Set this to true to force reading the file as bytes")]
    private readonly UpdatingKeyValue PrintBinary = new("binary");
    [NamedSetting("setlength", """
                               Set this to true to pass down the length of the file as well. Only do this if this file 
                               is big, and the only output component.
                               """)]
    private readonly UpdatingKeyValue SetContentLength = new("setlength");
    [NeverHappens]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens if the file was not found or accessible, or there was no output to write to.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        (constants, ConstantFilename).IsRereadRequired<object>(out var constantFilename);
        // if not set, we will let c# read the file in whatever encoding it has going on,
        // and then write it in the encoding the sink has going on.
        (constants, PrintBinary).IsRereadRequired<bool>(out var forceBinaryWriting);
        // we may be printing multiple files for concatenation, then we wouldnt wanna set content length
        (constants, SetContentLength).IsRereadRequired<bool>(out var setContentLength);

        FileInfo? selectedInfo = null;
        FileInfo? constantInfo = null;
        FileInfo? registerInfo = null;
        try
        {
            constantInfo = new FileInfo(constantFilename?.ToString());
        }
        catch (Exception ex)
        {

        }
        try
        {
            registerInfo = new FileInfo(interaction.Register.ToString());
        }
        catch (Exception ex)
        {

        }

        if (constantInfo?.Exists == true)
            selectedInfo = constantInfo;
        else if (registerInfo?.Exists == true)
            selectedInfo = registerInfo;
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "either set a constant filename, or provide one thru the primary value"));
            return;
        }

        if (interaction.TryGetClosest<ICheckUpdateRequiredInteraction>(out var checkUpdateRequiredInteraction) &&
            checkUpdateRequiredInteraction != null)
        {
            checkUpdateRequiredInteraction.IsRequired =
                checkUpdateRequiredInteraction.Original.LastSinkChangeTimestamp != selectedInfo.LastWriteTime.Ticks;
        }
        else if (interaction.TryGetClosest<ISinkingInteraction>(out var sinkingInteraction) &&
                   sinkingInteraction != null)
        {
            if (setContentLength == true)
                sinkingInteraction.SetContentLength64(selectedInfo.Length);

            sinkingInteraction.LastSinkChangeTimestamp = selectedInfo.LastWriteTime.Ticks;
            if (sinkingInteraction.TextEncoding is NoEncoding || forceBinaryWriting == true)
            {
                using (var fileStream = selectedInfo.OpenRead())
                {
                    fileStream.CopyTo(sinkingInteraction.SinkBuffer);
                }
            }
            else
            {
                using (var textStream = new StreamReader(selectedInfo.FullName, detectEncodingFromByteOrderMarks: true))
                {
                    try
                    {
                        sinkingInteraction.CopyTextFrom(textStream);
                    }
                    catch (ContentTypeMismatchException ex)
                    {
                        OnException?.Invoke(this, new CommonInteraction(interaction, ex));
                        return;
                    }
                }
            }
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "no sink found to write the file contents to"));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
