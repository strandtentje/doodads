namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Client;
#pragma warning disable 67
public class RequestBodySinkingInteraction(IInteraction interaction, Encoding encoding, Stream stream)
    : ISinkingInteraction
{
    private long ContentLength = -1;
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Encoding TextEncoding => encoding;
    public Stream SinkBuffer => stream;
    public string[] SinkContentTypePattern => ["*/*"];
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; } = -1;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength) => this.ContentLength = contentLength;
}