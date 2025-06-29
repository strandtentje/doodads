#nullable enable
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpHeadInteraction : IInteraction
{
    public readonly string RouteString, QueryString, Method, RemoteIp, RequestTime;
    public HttpHeadInteraction(IInteraction parent, HttpListenerContext context)
    {
        var urlHalves = context.Request.RawUrl?.Split("?") ?? ["/", ""];
        this.Context = context;
        this.Stack = parent;
        this.Register = context.Request.RawUrl ?? "";
        this.RouteString = urlHalves.ElementAtOrDefault(0) ?? "";
        this.QueryString = urlHalves.ElementAtOrDefault(1) ?? "";
        this.Method = context.Request.HttpMethod.ToUpper();
        this.RemoteIp = context.Request.RemoteEndPoint.ToString();
        this.RequestTime = DateTime.Now.ToString("yyMMdd HH:mm:ss");
        this.Memory = new SwitchingDictionary(["method", "query", "url", "remoteip", "requesttime"], x => x switch
        {
            "method" => Method,
            "query" => QueryString,
            "url" => RouteString,
            "remoteip" => RemoteIp,
            "requesttime" => RequestTime,
            _ => null
        });
    }
    public HttpListenerContext Context { get; set; }
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
}