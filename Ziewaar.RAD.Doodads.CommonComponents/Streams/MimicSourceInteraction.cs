using System.Text;

namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;

public class MimicSourceInteraction(IInteraction parent, ISourcingInteraction original, Stream overrideStream)
    : ISourcingInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public Stream SourceBuffer => overrideStream;
    public Encoding TextEncoding => original.TextEncoding;
    public string SourceContentTypePattern => original.SourceContentTypePattern;
    public long SourceContentLength => original.SourceContentLength;
}

public class MimicSinkInteraction(
    IInteraction parent,
    ISinkingInteraction original,
    Encoding encoding,
    Stream overrideStream)
    : ISinkingInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public Encoding TextEncoding => encoding;
    public Stream SinkBuffer => overrideStream;
    public string[] SinkContentTypePattern => original.SinkContentTypePattern;

    public string SinkTrueContentType
    {
        get => original.SinkTrueContentType;
        set => original.SinkTrueContentType = value;
    }

    public long LastSinkChangeTimestamp
    {
        get => original.LastSinkChangeTimestamp;
        set => original.LastSinkChangeTimestamp = value;
    }

    public string Delimiter => original.Delimiter;

    public void SetContentLength64(long contentLength) => original.SetContentLength64(contentLength);
}