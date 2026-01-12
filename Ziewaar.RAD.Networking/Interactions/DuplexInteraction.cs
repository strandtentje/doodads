using System.Text;

namespace Ziewaar.RAD.Networking;

public class DuplexInteraction(IInteraction interaction, Stream duplexStream) : ISourcingInteraction, ISinkingInteraction, IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Stream DuplexStream => duplexStream;
    Stream ISourcingInteraction.SourceBuffer => duplexStream;
    Encoding ISourcingInteraction.TextEncoding => NoEncoding.Instance;
    Stream ISinkingInteraction.SinkBuffer => duplexStream;
    string[] ISinkingInteraction.SinkContentTypePattern => ["*/*"];
    string? ISinkingInteraction.SinkTrueContentType { get; set; } = "application/octet-stream";
    long ISinkingInteraction.LastSinkChangeTimestamp { get; set; } = DateTime.Now.Ticks;
    string ISinkingInteraction.Delimiter => "";
    void ISinkingInteraction.SetContentLength64(long contentLength)
    {
        
    }
    string ISourcingInteraction.SourceContentTypePattern => "application/octet-stream";
    long ISourcingInteraction.SourceContentLength => -1;
    Encoding ISinkingInteraction.TextEncoding => NoEncoding.Instance;
}