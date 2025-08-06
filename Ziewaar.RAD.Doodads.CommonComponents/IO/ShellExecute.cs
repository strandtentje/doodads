#nullable enable
#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("System & IO")]
[Title("Open File or Folder in System Shell")]
[Description("""
    Open a file or folder using the OS appropriate app.
    """)]
public class ShellExecute : IService
{
    [EventOccasion("When file was opened.")]
    public event CallForInteraction? OnThen;
    [EventOccasion("Sink filename here.")]
    public event CallForInteraction? OnElse;
    [NeverHappens]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var tsi = new TextSinkingInteraction(interaction);
        OnElse?.Invoke(this, tsi);
        FileOpener.OpenWithDefaultApp(tsi.ReadAllText());
        OnThen?.Invoke(this, interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}
