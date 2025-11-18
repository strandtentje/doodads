#nullable enable
namespace Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

public interface IHttpHeadInteraction : IInteraction
{
    string RouteString { get; }
    string QueryString { get; }
    string Method { get; }
    string RemoteIp { get; }
    string RequestTime { get; }
    string RemotePort { get; }
    string RequestLocale { get; }
}