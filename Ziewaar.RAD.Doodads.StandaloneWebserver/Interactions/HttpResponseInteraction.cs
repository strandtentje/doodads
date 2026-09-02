using Ejije.Logging;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpResponseInteraction(
    IInteraction parent,
    HttpListenerContext context) : IHttpOutgoingResponseInteraction
{
    public string[] SinkContentTypePattern => context.Request.AcceptTypes ?? ["*/*"];
    public Encoding TextEncoding => context.Request.ContentEncoding ?? Encoding.UTF8;
    public CookieCollection OutgoingCookies => context.Response.Cookies;
    public Stream SinkBuffer => new ProtectedStream(context.Response.OutputStream, context);
    public string? SinkTrueContentType
    {
        get => context.Response.ContentType;
        set => context.Response.ContentType = value;
    }
    public long LastSinkChangeTimestamp { get; set; } = GlobalStopwatch.Instance.ElapsedTicks;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength) => context.Response.ContentLength64 = contentLength;
    private static readonly Log
        RedirectIn = Log.Tech("Setting redirect {code} to {url}..."),
        RedirectOut = Log.Tech("Done setting redirect code and Location: {code} - {location}");
    public void RedirectTo(string url, bool preservePost = false)
    {
        var desiredCode = preservePost ? (int)HttpStatusCode.TemporaryRedirect : (int)HttpStatusCode.SeeOther;
        Log.Post(RedirectIn, desiredCode, url);
        context.Response.StatusCode = desiredCode;
        context.Response.RedirectLocation = url;
        Log.Post(RedirectOut, context.Response.StatusCode, context.Response.RedirectLocation);
    }
    public int StatusCode
    {
        get => context.Response.StatusCode;
        set => context.Response.StatusCode = value;
    }
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}