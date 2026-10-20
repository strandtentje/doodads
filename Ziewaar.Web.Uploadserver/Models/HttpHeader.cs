namespace Ziewaar.Web.Uploadserver.Models;

public class HttpHeader : IEnumerable<HttpHeaderProperty>
{
    public readonly string Name;
    public readonly string MainValue;
    public readonly IEnumerable<HttpHeaderProperty> Properties;
    private static readonly char[]
        Colon = new char[] { ':' },
        SemiColon = new char[] { ';' },
        EqSign = new char[] { '=' };

    public HttpHeader(string name, string mainValue,
        IEnumerable<HttpHeaderProperty> properties)
    {
        this.Name = name;
        this.MainValue = mainValue;
        this.Properties = properties;
    }

    public static HttpHeader Parse(string line)
    {
        var parts = line.Split(Colon, 2, StringSplitOptions.RemoveEmptyEntries);
        var trimmedParts = parts.Select(x => x.Trim()).ToArray();
        var name = trimmedParts.ElementAtOrDefault(0);
        var values = trimmedParts.ElementAtOrDefault(1);
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(values))
            throw new ArgumentException("malformed header");
        var valueParts = values.Split(SemiColon,
            StringSplitOptions.RemoveEmptyEntries).ToArray();
        var mainValue = valueParts.ElementAtOrDefault(0);
        if (string.IsNullOrEmpty(mainValue))
            throw new ArgumentException("malformed header value");
        var properties = valueParts.Skip(1).
            Select(x => HttpHeaderProperty.Parse(x)).ToList();
        return new HttpHeader(name, mainValue, properties);
    }

    public IEnumerator<HttpHeaderProperty> GetEnumerator()
    {
        return Properties.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
}