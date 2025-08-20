using System.Net.Http.Headers;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
#pragma warning disable 67
public class HttpResponseSourcingInteraction(IInteraction cause, HttpResponseMessage response) : ISourcingInteraction
{
    public IInteraction Stack => cause;
    public object Register => response.StatusCode;
    public IReadOnlyDictionary<string, object> Memory => cause.Memory;
    public Stream SourceBuffer => response.Content.ReadAsStream();

    public Encoding TextEncoding => response.Content.Headers.ContentType?.CharSet is { } charsetName
        ? Encoding.GetEncoding(charsetName)
        : NoEncoding.Instance;

    public string SourceContentTypePattern =>
        response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

    public long SourceContentLength => response.Content.Headers.ContentLength ?? -1;
    public HttpResponseHeaders Headers => response.Headers;
}