#nullable enable
using System.Text;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions;

public class HttpInteraction(
    IInteraction parent,
    HttpListenerContext context) :
    ISourcingInteraction, ISinkingInteraction
    
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
    IReadOnlyDictionary<string, object> IInteraction.Variables { get; } = 

    public long LastSinkChangeTimestamp { get; set; }
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

    public IInteraction Stack => parent;
    public object Register => context.Request.RawUrl ?? "";
    public IReadOnlyDictionary<string, object> Memory = new SortedList<string, object>
    {
        { "method", context.Request.HttpMethod },
        { "query", context.Request.QueryString },
        { "url", context.Request.RawUrl?? "/" },
        { "cookies", context.Request.Cookies.}
    };
    public Stream SourceBuffer => context.Request.InputStream;
    public Encoding TextEncoding => context.Request.ContentEncoding ?? Encoding.UTF8;
    public Stream SinkBuffer => context.Response.OutputStream;
    public string[] SinkContentTypePattern => context.Request.AcceptTypes ?? ["*/*"];
    public string? SinkTrueContentType
    {
        get => context.Response.ContentType;
        set => context.Response.ContentType = value;
    }
    public string SourceContentTypePattern => context.Request.ContentType ?? "*/*";
    public long SourceContentLength => context.Request.ContentLength64;
}
