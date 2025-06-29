namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;
public class NumericComponent(string componentName) : IRouteComponent
{
    public string Name => componentName;
    public static bool TryParse(string component, [NotNullWhen(true)] out IRouteComponent? result)
    {
        if (!component.StartsWith("{#") || !component.EndsWith("#}"))
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
        result = new NumericComponent(componentName);
        return true;
    }
    public bool TryParseSegment(string requestRouteComponent, SortedList<string, object> urlVariables, out string verbatim)
    {
        if (decimal.TryParse(HttpUtility.UrlDecode(requestRouteComponent), out decimal candidateValue))
        {
            urlVariables[Name] = candidateValue;
            verbatim = HttpUtility.UrlEncode(urlVariables[Name].ToString()!);
            return true;
        }
        else
        {
            verbatim = "";
            return false;
        }
    }
}