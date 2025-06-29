namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
public class TextualComponent(string componentName) : IRouteComponent
{
    public string Name => componentName;
    public static bool TryParse(string component, [NotNullWhen(true)] out IRouteComponent? result)
    {
        if (!component.StartsWith("{$") || !component.EndsWith("$}"))
        {
            result = null;
            return false;
        }
        var componentName = component.Substring(2, component.Length - 4).Trim();
        if (string.IsNullOrWhiteSpace(componentName))
        {
            result = null;
            return false;
        }
        result = new TextualComponent(componentName);
        return true;
    }
    public bool TryParseSegment(string requestRouteComponent, SortedList<string, object> urlVariables, out string verbatim)
    {
        urlVariables[Name] = HttpUtility.UrlDecode(requestRouteComponent);
        verbatim = HttpUtility.UrlEncode(urlVariables[Name].ToString()!);
        return true;
    }
}