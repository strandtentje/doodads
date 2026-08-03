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
        if (SinkBuffer is ProxyBufferStream)
            this.DeferredContentLength = contentLength;
        else
            TrueSink.SetContentLength64(contentLength);
    }
    public Stream SinkBuffer { get; private set; }
    public string? SinkTrueContentType
    {
        get => field;
        set
        {
            field = value;
            if (SinkBuffer is not ProxyBufferStream)
            {
                TrueSink.SinkTrueContentType = value;
            }
        }
    }
    public long LastSinkChangeTimestamp { get; set; }
    public void Flush()
    {
        if (SinkBuffer is ProxyBufferStream pbs)
        {
            if (DeferredContentLength != -1)
                TrueSink.SetContentLength64(DeferredContentLength);
            pbs.FinalFlush();
            TrueSink.SinkTrueContentType = SinkTrueContentType;
            TrueSink.LastSinkChangeTimestamp = LastSinkChangeTimestamp;
        }
    }
    public void Bypass()
    {
        if (SinkBuffer is ProxyBufferStream pbs)
        {
            SinkBuffer = TrueSink.SinkBuffer;
        }
    }
}