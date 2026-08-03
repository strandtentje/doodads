namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Routing;

public class RelativeRouteInteraction(
    IInteraction parent,
    HttpHeadInteraction httpHead,
    SortedList<string, object> routeVars,
    string currentLocation,
    IEnumerable<string> remainingUrl) : IInteraction, IRelativeRouteInteraction
{
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => new RelativeRouteDictionary(routeVars, CurrentLocation, remainingUrl);
    public string CurrentLocation => currentLocation;
    public IEnumerable<string> Remaining => remainingUrl;
    public HttpHeadInteraction HttpHead => httpHead;
}

public class RelativeRouteDictionary(SortedList<string, object> routeVars, string current, IEnumerable<string> remaining) : IReadOnlyDictionary<string, object>
{
    private const string
        CURRENT_LOCATION = "currentlocation",
        URL_PEEK_PREFIX = "urlpeek";
    public bool TryGetValue(string key, [MaybeNullWhen(false)] out object value)
    {
        if (StringComparer.OrdinalIgnoreCase.Equals(key, CURRENT_LOCATION))
        {
            value = current;
            return true;
        }
        else if (key.StartsWith(URL_PEEK_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            var remainder = $"0{key.Substring(URL_PEEK_PREFIX.Length)}";
            if (uint.TryParse(remainder, CultureInfo.InvariantCulture, out var peekNumber))
            {
                foreach (var item in remaining)
                {
                    if (peekNumber == 0)
                    {
                        value = item;
                        return true;
                    }
                    else
                    {
                        peekNumber--;
                    }
                }
                value = "";
                return false;
            }
            else
            {
                value = "";
                return false;
            }
        }
        else
        {
            return routeVars.TryGetValue(key, out value);
        }
    }
    public object this[string key] => TryGetValue(key, out var v) ? v : throw new KeyNotFoundException();
    public IEnumerable<string> Keys
    {
        get
        {
            yield return CURRENT_LOCATION;
            var rc = remaining.Count();
            if (rc > 0)
                yield return URL_PEEK_PREFIX;
            foreach (var item in routeVars)
                yield return item.Key;
            for (int i = 0; i < rc; i++)
                yield return $"{URL_PEEK_PREFIX}{i}";
        }
    }
    public IEnumerable<object> Values => Keys.Select(x => this[x]);
    public int Count
    {
        get
        {
            var rc = remaining.Count();
            if (rc == 0)
            {
                return 1 + routeVars.Count;
            } else
            {
                return 2 + routeVars.Count + rc;
            }
        }
    }
    public bool ContainsKey(string key) => TryGetValue(key, out var _);
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator() =>
        Keys.Select(x => new KeyValuePair<string, object>(x, this[x])).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}