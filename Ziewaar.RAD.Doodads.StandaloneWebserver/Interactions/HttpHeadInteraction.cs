namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;
public class HttpHeadInteraction : IInteraction, IHttpHeadInteraction
{
    string IHttpHeadInteraction.RouteString => RouteString;
    string IHttpHeadInteraction.QueryString => QueryString;
    string IHttpHeadInteraction.Method => Method;
    string IHttpHeadInteraction.RemoteIp => RemoteIp;
    string IHttpHeadInteraction.RemotePort => RemotePort;
    string IHttpHeadInteraction.RequestTime => RequestTime;
    string IHttpHeadInteraction.RequestLocale => RequestLocale;
    public readonly string 
        RouteString, QueryString, Method, RemoteIp, RequestTime, RemotePort, RequestLocale;
    public HttpHeadInteraction(IInteraction parent, HttpListenerContext context,
        Services.ExpandedPrefixes expandedPrefixes)
    {
        var urlHalves = context.Request.RawUrl?.Split("?") ?? ["/", ""];
        this.Context = context;
        this.Stack = parent;
        this.Register = context.Request.RawUrl ?? "";
        this.RouteString = urlHalves.ElementAtOrDefault(0) ?? "";
        this.QueryString = urlHalves.ElementAtOrDefault(1) ?? "";
        this.Method = context.Request.HttpMethod.ToUpper();
        this.RemoteIp = context.Request.RemoteEndPoint.Address.ToString();
        this.RemotePort = context.Request.RemoteEndPoint.Port.ToString();
        this.RequestTime = DateTime.Now.ToString("yyMMdd HH:mm:ss");
        this.RequestLocale =
            context.Request.Headers["Accept-Language"]?.Split(',').ElementAtOrDefault(0)?.Trim().ToLower() ??
            CultureInfo.CurrentCulture.Name;
        this.Memory = new SwitchingDictionary(
        [
            "requestlocale", "method", "query", "unescapedquery", "url", "remoteip", "requesttime", "loopbackurl",
            "localipurl", "localnameurl"
        ], x => x switch
        {
            "requestlocale" => RequestLocale,
            "method" => Method,
            "query" => QueryString,
            "unescapedquery" => HttpUtility.UrlDecode(QueryString, Encoding.UTF8),
            "url" => RouteString,
            "remoteip" => RemoteIp,
            "remoteport" => RemotePort,
            "requesttime" => RequestTime,
            "loopbackurl" => expandedPrefixes.LoopbackURL.TrimEnd('/'),
            "localipurl" => expandedPrefixes.LocalIPURL.TrimEnd('/'),
            "localnameurl" => expandedPrefixes.LocalHostnameURL.Trim('/'),
            _ => throw new KeyNotFoundException()
        });
    }
    public HttpListenerContext Context { get; set; }
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
}