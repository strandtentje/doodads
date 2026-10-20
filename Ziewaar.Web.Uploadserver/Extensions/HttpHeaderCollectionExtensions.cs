namespace Ziewaar.Web.Uploadserver.Extensions;

public static class HttpHeaderCollectionExtensions
{
    public static HttpHeader ByName(this HttpHeaderCollection collection, string name)
    {
        var matches = collection.
            Where(x => StringComparer.OrdinalIgnoreCase.Equals(x.Name, name));
        return matches.ElementAtOrDefault(0);
    }

    public static HttpHeader GetContentDisposition(
        this HttpHeaderCollection collection)
    {
        return collection.ByName(HeaderNames.ContentDisposition);
    }

    public static HttpHeader GetContentType(
        this HttpHeaderCollection collection)
    {
        return collection.ByName(HeaderNames.ContentType);
    }

    public static HttpHeader GetContentTransferEncoding(
        this HttpHeaderCollection collection)
    {
        return collection.ByName(HeaderNames.ContentTransferEncoding);
    }
}