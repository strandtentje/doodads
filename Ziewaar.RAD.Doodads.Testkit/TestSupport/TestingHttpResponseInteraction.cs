using System.IO.Pipelines;
using System.Net;
using System.Text;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Testkit;
public class TestingHttpResponseInteraction : ISinkingInteraction, IDisposable
{
    public readonly Pipe Pipe;
    public readonly List<long> ContentLengthReports = new List<long>();
    public TestingHttpResponseInteraction(IInteraction parent, string[]? acceptMimes)
    {
        this.Stack = parent;
        this.Register = parent.Register;
        this.Memory = parent.Memory;
        this.SinkContentTypePattern = acceptMimes ?? ["*/*"];
        this.Pipe = new Pipe(PipeOptions.Default);
        this.SinkBuffer = Pipe.Writer.AsStream(leaveOpen: true);
    }
    public string? RedirectionURL = null;
    public string[] SinkContentTypePattern { get; private set; }
    public CookieCollection OutgoingCookies { get; private set; } = new CookieCollection();
    public Encoding TextEncoding => Encoding.UTF8;
    public Stream SinkBuffer { get; }
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; } = GlobalStopwatch.Instance.ElapsedTicks;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength) => ContentLengthReports.Add(contentLength);
    public void RedirectTo(string url) => this.RedirectionURL = url;
    public int StatusCode { get; set; } = 200;
    public IInteraction Stack { get; private set; }
    public object Register { get; private set; }
    public IReadOnlyDictionary<string, object> Memory { get; private set; }
    public void Dispose()
    {
        this.SinkBuffer.Dispose();
    }
}