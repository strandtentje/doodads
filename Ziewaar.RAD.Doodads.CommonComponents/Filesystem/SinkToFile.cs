#nullable enable
namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;
#pragma warning disable 67
[Category("System & IO")]
[Title("Save to File")]
[Description("""Sink data into file""")]
public class SinkToFile : IService
{
    [EventOccasion("Sink filename here")] public event CallForInteraction? OnThen;

    [EventOccasion("Sink data for file here")]
    public event CallForInteraction? OnElse;

    [NeverHappens] public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        OnThen?.Invoke(this, tsi);
        var filename = tsi.ReadAllText();

        if (File.Exists(filename)) File.Delete(filename);

        var fileStream = File.OpenWrite(filename);
        var bsi = new FileSinkingInteraction(interaction, fileStream);
        OnElse?.Invoke(this, bsi);
        fileStream.Flush();
        fileStream.Close();
        fileStream.Dispose();
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}