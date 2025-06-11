#nullable enable
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpHeadInteraction(IInteraction parent, HttpListenerContext context) : IInteraction
{
    public IInteraction Stack => parent;
    public object Register => context.Request.RawUrl ?? "";
    public IReadOnlyDictionary<string, object> Memory { get; } = new SortedList<string, object>
    {
        { "method", context.Request.HttpMethod },
        { "query", context.Request.QueryString },
        { "url", context.Request.RawUrl ?? "/" },
        { "remoteip", context.Request.RemoteEndPoint.ToString() },
        { "requesttime", DateTime.Now.ToString("yyMMdd HH:mm:ss") }
    };
}