using System.Globalization;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Testkit;
public class TestingHttpHeadInteraction : IInteraction
{
    public readonly string RouteString, QueryString, Method, RemoteIp, RequestTime, RemotePort, RequestLocale;
    public TestingHttpHeadInteraction(IInteraction parent, HttpMethod method, string rawUrl)
    {
        var urlHalves = rawUrl.Split("?") ?? ["/", ""];
        this.Stack = parent;
        this.Register = rawUrl ?? "";
        this.RouteString = urlHalves.ElementAtOrDefault(0) ?? "";
        this.QueryString = urlHalves.ElementAtOrDefault(1) ?? "";
        this.Method = method.ToString().ToUpper();
        this.RemoteIp = "127.0.0.1";
        this.RemotePort = "14524";
        this.RequestTime = DateTime.Now.ToString("yyMMdd HH:mm:ss");
        this.RequestLocale = CultureInfo.InvariantCulture.Name;
        this.Memory = new SwitchingDictionary([
            "requestlocale", "method", "query", 
            "url", "remoteip", "requesttime", "remoteport"], x => x switch
        {
            "requestlocale" => RequestLocale,
            "method" => Method,
            "query" => QueryString,
            "url" => RouteString,
            "remoteip" => RemoteIp,
            "remoteport" => RemotePort,
            "requesttime" => RequestTime,
            _ => throw new KeyNotFoundException()
        });
    }
    
    public IInteraction Stack { get; }
    public object Register { get; }
    public IReadOnlyDictionary<string, object> Memory { get; }
}