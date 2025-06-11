using System.Text;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpRequestInteraction(
    IInteraction parent,
    HttpListenerContext context) :
    ISourcingInteraction
{
    public CookieCollection IncomingCookies => context.Request.Cookies;
    public Stream SourceBuffer => context.Request.InputStream;
    public Encoding TextEncoding => context.Request.ContentEncoding ?? Encoding.UTF8;
    public string SourceContentTypePattern => context.Request.ContentType ?? "*/*";
    public long SourceContentLength => context.Request.ContentLength64;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}