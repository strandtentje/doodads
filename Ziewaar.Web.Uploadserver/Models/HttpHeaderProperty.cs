namespace Ziewaar.Web.Uploadserver.Models;

public class HttpHeaderProperty
{
    private static readonly char[] EqSign = new char[] { '=' };
    public readonly string Name, Value;
    public HttpHeaderProperty(string name, string value)
    {
        this.Name = name;
        this.Value = value;
    }

    public static HttpHeaderProperty Parse(string assignment)
    {
        var halves = assignment.
            Split(EqSign, 2, StringSplitOptions.RemoveEmptyEntries).
            Select(x => x.Trim()).
            Where(x => !string.IsNullOrEmpty(x)).
            ToArray();

        var name = halves.ElementAtOrDefault(0);
        var value = halves.ElementAtOrDefault(1);

        if (name == null) name = "";
        if (value == null) value = "";

        return new HttpHeaderProperty(name, value);
    }
}