using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;
using Ziewaar.Web.Uploadserver;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.FastMultipart;

public class UploadFormHttpLifecycle(
    IHttpHeadInteraction head,
    ICookieInteraction cookie,
    IHttpIncomingRequestInteraction request,
    IHttpOutgoingResponseInteraction response,
    ISinkingInteraction sink,
    BufferSinkInteraction? plugBuffer,
    string actionUrl) : IHttpLifecycleWrapper
{
    public string UpperHttpMethod => head.Method.ToUpper();
    public string SessionId => cookie.Register.ToString() ?? throw new BasicException("missing cookie");

    public Encoding? ResponseEncoding
    {
        get => sink.TextEncoding;
        set { }
    }

    public Stream ResponseStream
    {
        get => sink.SinkBuffer;
    }

    public Stream RequestStream
    {
        get => request.SourceBuffer;
    }

    public string? RequestContentType
    {
        get => request.RawContentType;
    }

    public long RequestContentLength
    {
        get => request.SourceContentLength;
    }
    public string ActionUrl => actionUrl;

    public string ActionBeforeHash => field ??= actionUrl.Split('#', 2, (StringSplitOptions)3).ElementAtOrDefault(0) ?? "";
    public string ActionAfterHash => field ??= actionUrl.Split('#', 2, (StringSplitOptions)3).ElementAtOrDefault(1) ?? "";

    private IReadOnlyDictionary<string, string> QueryParameters => field ??=
        HttpUtility.HtmlDecode(head.QueryString).Split('&', (StringSplitOptions)3)
            .Select(assignment => (eqPos: assignment.IndexOf('='), assignment)).Where(x => x.eqPos > 0)
            .Select(pair => (key: pair.assignment.Substring(0, pair.eqPos),
                value: pair.assignment.Substring(pair.eqPos + 1)))
            .Select(pair => (key: HttpUtility.UrlDecode(pair.key), value: HttpUtility.UrlDecode(pair.value)))
            .DistinctBy(x => x.key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(x => x.key, x => x.value, StringComparer.OrdinalIgnoreCase);

    public void Bypass() => plugBuffer?.Bypass();

    public string GetFromQueryString(string key) =>
        QueryParameters.TryGetValue(key, out var value) ? value : string.Empty;

    public void SetCacheControl(string value) => response.SetCacheControl(value);

    public void SetKeepAlive(bool ka) => response.SetKeepAlive(ka);

    public void TrySetResponseHeader(long contentLength, string contentType)
    {
        if (sink.SinkTrueContentType == null || sink.SinkTrueContentType != contentType)
            sink.SinkTrueContentType = contentType;
    }
}