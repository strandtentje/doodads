#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;
#pragma warning disable 67
[Category("System & IO")]
[Title("Load from file")]
[Description("""Sink data from file""")]
public class SinkFromFile : IService
{
    [PrimarySetting("""
                    Set this to binary in case we're not reading a text file, or we will not be
                    reading the file like a text file. Otherwise, encoding will be deduced.
                    """)]
    private readonly UpdatingPrimaryValue PipeStyleConstant = new();
    private bool IsCurrentlyBinary = false;

    [EventOccasion("Sink filename here")] public event CallForInteraction? OnThen;

    [EventOccasion("When file wasn't found")]
    public event CallForInteraction? OnElse;

    [EventOccasion("When there wasn't any sink.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, PipeStyleConstant).IsRereadRequired(out string? pipeStyle))
            IsCurrentlyBinary = pipeStyle == "binary";

        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var filename = tsi.ReadAllText();
        var fileInfo = new FileInfo(filename);
        if (fileInfo.Exists)
        {
            if (!interaction.TryGetClosest(out ISinkingInteraction? sinkingInteraction)
                || sinkingInteraction == null)
            {
                OnException?.Invoke(this, interaction.AppendRegister("Sink required to put file contents into."));
                return;
            }

            if (IsCurrentlyBinary)
            {
                using var readStream = fileInfo.OpenRead();
                readStream.CopyTo(sinkingInteraction.SinkBuffer);
            }
            else
            {
                using var streamReader = new StreamReader(fileInfo.FullName);
                var textChars = streamReader.ReadToEnd();
                var textBytes = sinkingInteraction.TextEncoding.GetBytes(textChars);
                sinkingInteraction.SinkBuffer.Write(textBytes, 0, textBytes.Length);
            }
        }
        else
        {
            OnElse?.Invoke(this, interaction);
        }
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
