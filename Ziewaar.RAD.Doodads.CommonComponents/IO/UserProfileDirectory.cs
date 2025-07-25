#nullable enable
using System.Text;

#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.CommonComponents.IO;

[Category("Filesystem")]
[Title("User Directory")]
[Description("""Put path to /home/user or C:\\Users\\User into register""")]
public class UserProfileDirectory : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction) =>
        OnThen?.Invoke(this, new CommonInteraction(
            interaction,
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)));
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}


public class FileSinkingInteraction(IInteraction interaction, Stream stream) : ISinkingInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Encoding TextEncoding => NoEncoding.Instance;
    public Stream SinkBuffer => stream;
    public string[] SinkContentTypePattern => ["*/*"];
    public string? SinkTrueContentType { get; set; } = "application/octet-stream";
    public long LastSinkChangeTimestamp { get; set; } = long.MinValue;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength)
    {
        
    }
}