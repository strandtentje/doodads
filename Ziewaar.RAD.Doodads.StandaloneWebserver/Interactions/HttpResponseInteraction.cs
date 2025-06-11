using System.Text;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpResponseInteraction(
    IInteraction parent,
    HttpListenerContext context) : ISinkingInteraction
{
    public string[] SinkContentTypePattern => context.Request.AcceptTypes ?? ["*/*"];
    public Encoding TextEncoding => context.Request.ContentEncoding ?? Encoding.UTF8;
    public CookieCollection OutgoingCookies => context.Response.Cookies;
    public Stream SinkBuffer => context.Response.OutputStream;
    public string? SinkTrueContentType
    {
        get => context.Response.ContentType;
        set => context.Response.ContentType = value;
    }
    public long LastSinkChangeTimestamp { get; set; } = GlobalStopwatch.Instance.ElapsedTicks;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength) => context.Response.ContentLength64 = contentLength;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}