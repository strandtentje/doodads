#nullable enable
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpHeadInteraction : IInteraction
{
    public HttpHeadInteraction(IInteraction parent, HttpListenerContext context)
    {
        var urlHalves = context.Request.RawUrl?.Split("?") ?? ["/", ""];
        this.Context = context;
        this.Stack = parent;
        this.Register = context.Request.RawUrl ?? "";
        this.Memory = new SortedList<string, object>
        {
            { "method", context.Request.HttpMethod.ToUpper() },
            { "query", urlHalves.ElementAtOrDefault(1) ?? "" },
            { "url", urlHalves.ElementAtOrDefault(0) ?? "" },
            { "remoteip", context.Request.RemoteEndPoint.ToString() },
            { "requesttime", DateTime.Now.ToString("yyMMdd HH:mm:ss") }
        };
    }
    public HttpListenerContext Context { get; set; }
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
}