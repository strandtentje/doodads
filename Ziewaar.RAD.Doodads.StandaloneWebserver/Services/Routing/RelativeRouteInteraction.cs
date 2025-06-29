namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
public class RelativeRouteInteraction(
    IInteraction parent,
    HttpHeadInteraction httpHead,
    SortedList<string, object> routeVars,
    string currentLocation,
    IEnumerable<string> remainingUrl) : IInteraction
{
    private const string CURRENT_LOCATION = "currentlocation";
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => new SwitchingDictionary(
        routeVars.Keys.Append(CURRENT_LOCATION).ToArray(),
        x => x == CURRENT_LOCATION ? currentLocation : routeVars[x]);
    public string CurrentLocation => currentLocation; 
    public IEnumerable<string> Remaining = remainingUrl;
    public HttpHeadInteraction HttpHead => httpHead;
}