using System.Net;
using System.Text;

namespace Ziewaar.RAD.Doodads.Testkit;
public static class QueryStringBuilder
{
    public static string ToQueryString(this IReadOnlyDictionary<string, string> parameters)
    {
        var builder = new StringBuilder();
        foreach (var kvp in parameters)
        {
            if (builder.Length > 0)
                builder.Append('&');

            builder.Append(WebUtility.UrlEncode(kvp.Key));
            builder.Append('=');
            builder.Append(WebUtility.UrlEncode(kvp.Value));
        }

        return builder.ToString();
    }
}