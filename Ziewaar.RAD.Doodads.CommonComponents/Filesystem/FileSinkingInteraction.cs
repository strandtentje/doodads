#nullable enable
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Filesystem;
#pragma warning disable 67
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