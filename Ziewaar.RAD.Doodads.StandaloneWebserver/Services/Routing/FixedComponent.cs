namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
public class FixedComponent(string exactMatch) : IRouteComponent
{
    public string Name => exactMatch; 
    private static bool IsUrlSafe(char c)
    {
        return (c >= 'A' && c <= 'Z') ||
               (c >= 'a' && c <= 'z') ||
               (c >= '0' && c <= '9') ||
               c == '-' || c == '_' || c == '.' || c == '~';
    }
    public static bool TryParse(string component, [NotNullWhen(true)] out IRouteComponent? result)
    {
        if (!component.All(IsUrlSafe))
        {
            result = null;
            return false;
        }
        var componentName = component.Trim();
        if (string.IsNullOrWhiteSpace(componentName))
        {
            result = null;
            return false;
        }
        result = new FixedComponent(component);
        return true;
    }
    public bool TryParseSegment(string requestRouteComponent, SortedList<string, object> urlVariables, out string verbatim)
    {
        verbatim = HttpUtility.UrlEncode(HttpUtility.UrlDecode(Name));
        return requestRouteComponent == Name;
    }
}