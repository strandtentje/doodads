using System.Text;
using Ziewaar.Network.Protocol;

namespace Ziewaar.RAD.Networking;

public class MultiplexedInteraction(IInteraction repeater, ProtocolOverStream interactionProtocol, Stream asStream, Lock protocolLock)
    : IInteraction, ISourcingInteraction, ISinkingInteraction
{
    public IInteraction Stack => repeater;
    public object Register => repeater.Register;

    public IReadOnlyDictionary<string, object> Memory { get; } =
        new FallbackReadOnlyDictionary(new SideChannelDictionary(protocolLock, interactionProtocol), repeater.Memory);

    public Stream SourceBuffer => asStream;
    public Encoding TextEncoding => NoEncoding.Instance;
    public Stream SinkBuffer => asStream;
    public string[] SinkContentTypePattern { get; } = ["*/*"];
    public string? SinkTrueContentType { get; set; } = "application/octet-stream";
    public long LastSinkChangeTimestamp { get; set; } = DateTime.Now.Ticks;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength) { }
    public string SourceContentTypePattern { get; } = "*/*";
    public long SourceContentLength { get; } = -1;
}