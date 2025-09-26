using System.Net;
using System.Net.Sockets;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary;

namespace Ziewaar.RAD.Doodads.Cryptography;

#nullable enable
internal class TcpClientDuplexInteraction(IInteraction origin, TcpClient freshClient, NetworkStream networkStream) 
    : IInteraction, ISourcingInteraction, ISinkingInteraction
{
    public IInteraction Stack => origin;
    public object Register => origin.Register;
    public IReadOnlyDictionary<string, object> Memory => new SwitchingDictionary(["client-ip", "client-port"], key => key switch
    {
        "client-ip" => (freshClient.Client.RemoteEndPoint as IPEndPoint)?.Address?.ToString() ?? "",
        "client-port" => (freshClient.Client.RemoteEndPoint as IPEndPoint)?.Port ?? 0,
        _ => throw new KeyNotFoundException(),
    });
    Stream ISourcingInteraction.SourceBuffer => networkStream;
    Encoding ISourcingInteraction.TextEncoding => NoEncoding.Instance;
    Stream ISinkingInteraction.SinkBuffer => networkStream;
    string[] ISinkingInteraction.SinkContentTypePattern => ["*/*"];
    string? ISinkingInteraction.SinkTrueContentType { get; set; } = "application/octet-stream";
    long ISinkingInteraction.LastSinkChangeTimestamp { get; set; } = GlobalStopwatch.Instance.ElapsedTicks;
    string ISinkingInteraction.Delimiter => "";
    void ISinkingInteraction.SetContentLength64(long contentLength) { }
    string ISourcingInteraction.SourceContentTypePattern => "*/*";
    long ISourcingInteraction.SourceContentLength => -1;
    Encoding ISinkingInteraction.TextEncoding => NoEncoding.Instance;
}