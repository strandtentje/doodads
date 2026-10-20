namespace Ziewaar.Web.Uploadserver.Extensions;

public static class HttpHeaderExtensions
{
    public static bool HasMainValueCI(
        this HttpHeader header,
        string value)
    {
        if (header == null) return false;
        return StringComparer.OrdinalIgnoreCase.Equals(
            header.MainValue, value);
    }
    public static string GetPropValueCI(this HttpHeader header, string name)
    {
        if (header == null) return null;
        var value = header.Where(x => StringComparer.OrdinalIgnoreCase.Equals(
            x.Name, name)).Select(x => x.Value).ElementAtOrDefault(0);
        if (value == null) return null;
        value = value.Trim();
        if (value.Length < 3) return null; 
        // at least dbl quotes are expected, with something between
        // them. by that logic, anything shorter than 3 couldn't 
        // possibly be right
        if (!value.StartsWith("\"")) return null;
        if (!value.EndsWith("\"")) return null;
        // missing quotes no bueno
        value = value.Substring(1, value.Length - 2);
        // chop the quotes and check of any other bullshit
        if (value.Contains('"')) return null;
        // there's probably still bullshit in there but this
        // is good enough for now.
        return value;
    }
}