namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
public interface IRouteComponent
{
    bool TryParseSegment(string requestRouteComponent, SortedList<string, object> urlVariables, out string verbatim);
}