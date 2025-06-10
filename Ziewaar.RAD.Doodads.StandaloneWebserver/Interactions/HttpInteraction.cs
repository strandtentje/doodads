#nullable enable
using System.Text;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class HttpInteraction(
    IInteraction parent,
    HttpListenerContext context) :
    ISourcingInteraction, ISinkingInteraction
    
{

    public IInteraction Stack => parent;
    public object Register => context.Request.RawUrl ?? "";
    public IReadOnlyDictionary<string, object> Memory { get; } = new SortedList<string, object>
    {
        { "method", context.Request.HttpMethod },
        { "query", context.Request.QueryString },
        { "url", context.Request.RawUrl?? "/" },
    };
    public CookieCollection IncomingCookies => context.Request.Cookies;
    public CookieCollection OutgoingCookies => context.Response.Cookies;
    public Stream SourceBuffer => context.Request.InputStream;
    public Encoding TextEncoding => context.Request.ContentEncoding ?? Encoding.UTF8;
    public Stream SinkBuffer => context.Response.OutputStream;
    public string[] SinkContentTypePattern => context.Request.AcceptTypes ?? ["*/*"];
    public string? SinkTrueContentType
    {
        get => context.Response.ContentType;
        set => context.Response.ContentType = value;
    }
    public string SourceContentTypePattern => context.Request.ContentType ?? "*/*";
    public long SourceContentLength => context.Request.ContentLength64;
    public long LastSinkChangeTimestamp { get; set; } = GlobalStopwatch.Instance.ElapsedTicks;
    public string Delimiter { get; } ="";
    public void SetContentLength64(long contentLength) => context.Response.ContentLength64 = contentLength;
}
