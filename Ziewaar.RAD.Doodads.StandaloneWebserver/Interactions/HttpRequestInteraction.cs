namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class HttpRequestInteraction(
    IInteraction parent,
    HttpListenerContext context) :
    IHttpIncomingRequestInteraction
{
    public CookieCollection IncomingCookies => context.Request.Cookies;
    public Stream SourceBuffer => new ProtectedStream(context.Request.InputStream, context);
    public Encoding TextEncoding => context.Request.ContentEncoding ?? Encoding.UTF8;
    public string SourceContentTypePattern => context.Request.ContentType.GetBaseHeader();
    public long SourceContentLength => context.Request.ContentLength64;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
    public IReadOnlyDictionary<string, string> ContentTypeProperties =>
        context.Request.ContentType.GetHeaderProperties();
}

