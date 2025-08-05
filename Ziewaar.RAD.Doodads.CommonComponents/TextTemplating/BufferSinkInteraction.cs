#nullable enable
using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
public class BufferSinkInteraction : ISinkingInteraction
{
    private readonly IInteraction Parent;
    private readonly ISinkingInteraction TrueSink;
    private long DeferredContentLength = -1;
    public BufferSinkInteraction(IInteraction parent, ISinkingInteraction trueSink)
    {
        Parent = parent;
        TrueSink = trueSink;
        SinkBuffer = new ProxyBufferStream(trueSink.SinkBuffer);
        LastSinkChangeTimestamp = trueSink.LastSinkChangeTimestamp;
    }
    public IInteraction Stack => Parent;
    public object Register => Parent.Register;
    public IReadOnlyDictionary<string, object> Memory => Parent.Memory;
    public Encoding TextEncoding => TrueSink.TextEncoding;
    public string[] SinkContentTypePattern => TrueSink.SinkContentTypePattern;
    public string Delimiter => TrueSink.Delimiter;
    public void SetContentLength64(long contentLength)
    {
        this.DeferredContentLength = contentLength;
    }
    public Stream SinkBuffer { get; }
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; }
    public void Flush()
    {
        if (DeferredContentLength != -1)
            TrueSink.SetContentLength64(DeferredContentLength);
        ((ProxyBufferStream)SinkBuffer).FinalFlush();
        TrueSink.SinkTrueContentType = SinkTrueContentType;
        TrueSink.LastSinkChangeTimestamp = LastSinkChangeTimestamp;
    }
}