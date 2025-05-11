namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class HttpInteraction(
    IInteraction parent,
    HttpListenerContext context) :
    ISourcingInteraction<Stream>, ISinkingInteraction<Stream>,
    ISourcingInteraction<CookieCollection>, ISinkingInteraction<CookieCollection>,
    ISourcingInteraction<HttpMethod>, ISinkingInteraction<HttpStatusCodeChange>,
    IContentTypeSink, IContentTypeSource
{
    public IInteraction Parent => parent;
    ITaggedData<Stream> ISourcingInteraction<Stream>.TaggedData { get; } =
        new RequestBodyTaggedData(context.Request.InputStream);
    ITaggedData<Stream> ISinkingInteraction<Stream>.TaggedData { get; } =
        new ResponseBodyTaggedData(context.Response.OutputStream);
    ITaggedData<CookieCollection> ISourcingInteraction<CookieCollection>.TaggedData { get; } =
        new RequestingTaggedCookies(context.Request.Cookies);
    ITaggedData<CookieCollection> ISinkingInteraction<CookieCollection>.TaggedData { get; } =
        new RespondingTaggedCookies(context.Response.Cookies);
    ITaggedData<HttpMethod> ISourcingInteraction<HttpMethod>.TaggedData { get; } =
        new RequestMethodTaggedData(context.Request.HttpMethod);
    ITaggedData<HttpStatusCodeChange> ISinkingInteraction<HttpStatusCodeChange>.TaggedData { get; } =
        new ResponseStatusCodeTaggedData(context.Response);
    IReadOnlyDictionary<string, object> IInteraction.Variables { get; } = new SortedList<string, object>
{
    { "method", context.Request.HttpMethod },
    { "query", context.Request.QueryString?.AllKeys?.
        OfType<string>().
        ToDictionary(x => x, x =>
            context.Request.QueryString.GetValues(x) ?? []) ??
            new Dictionary<string,string[]>()
    },
    { "url", context.Request.RawUrl?? "/" }
};
    string ISinkingInteraction<Stream>.Delimiter => "";
    string ISinkingInteraction<CookieCollection>.Delimiter => "";
    string ISinkingInteraction<HttpStatusCodeChange>.Delimiter => "";
    string[] IContentTypeSink.Accept { get; } = context.Request.AcceptTypes ?? ["*"];
    string IContentTypeSink.ContentType
    {
        get => context.Response.ContentType ?? "application/octet-stream";
        set => context.Response.ContentType = value;
    }
    string IContentTypeSource.ContentType => context.Request.ContentType ?? "application/octet-stream";

    IInteraction IInteraction.Parent => throw new NotImplementedException();
}
