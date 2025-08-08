using System.Net.Http.Headers;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Client;
#pragma warning disable 67

public class HttpGetRequest : WebClientService
{
    private readonly UpdatingPrimaryValue RequestUrlConstant = new();
    private string? CurrentRequestUrl;

    protected override WebClientRequestHead GetRequestHead(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RequestUrlConstant).IsRereadRequired(out string? requestUrl))
            this.CurrentRequestUrl = requestUrl;
        if (string.IsNullOrWhiteSpace(this.CurrentRequestUrl))
            throw new Exception("URL required as primary constant");
        return new(System.Net.Http.HttpMethod.Get, CurrentRequestUrl, x => x);
    }

    protected override WebClientRequestBody? GetRequestBody(StampedMap constants, IInteraction interaction,
        Stream stream) => null;
}

public class HttpPostRequest : WebClientService
{
    [EventOccasion("Sink request body here.")]
    public event CallForInteraction? SinkBody;

    private readonly UpdatingPrimaryValue RequestUrlConstant = new();
    private readonly UpdatingKeyValue RequestEncodingConstant = new("encoding");
    private string? CurrentRequestUrl;
    private string? CurrentRequestEncodingWebname;

    protected override WebClientRequestHead GetRequestHead(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RequestUrlConstant).IsRereadRequired(out string? requestUrl))
            this.CurrentRequestUrl = requestUrl;
        if (string.IsNullOrWhiteSpace(this.CurrentRequestUrl))
            throw new Exception("URL required as primary constant");
        return new(System.Net.Http.HttpMethod.Get, CurrentRequestUrl, x => x);
    }

    protected override WebClientRequestBody? GetRequestBody(StampedMap constants, IInteraction interaction,
        Stream stream)
    {
        if ((constants, RequestEncodingConstant).IsRereadRequired(out string? requestEncoding))
            this.CurrentRequestEncodingWebname = requestEncoding;
        Encoding currentEncoding = string.IsNullOrWhiteSpace(CurrentRequestEncodingWebname)
            ? NoEncoding.Instance
            : Encoding.GetEncoding(CurrentRequestEncodingWebname);
        var rbsi = new RequestBodySinkingInteraction(interaction, currentEncoding);
        SinkBody?.Invoke(this, rbsi);
        return rbsi.CreateBody();
    }
}

public class RequestBodySinkingInteraction(IInteraction interaction, Encoding encoding)
    : ISinkingInteraction
{
    public Func<Stream> OnStreamRequested;
    private long ContentLength = -1;
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public Encoding TextEncoding => encoding;
    private Stream? _backingStream;
    public Stream SinkBuffer
    {
        get
        {
            if (_backingStream == null)
            {
                _backingStream = OnStreamRequested();
            }

            return _backingStream;
        }
    }
    public string[] SinkContentTypePattern => ["*/*"];
    public string? SinkTrueContentType { get; set; }
    public long LastSinkChangeTimestamp { get; set; } = -1;
    public string Delimiter { get; } = "";
    public void SetContentLength64(long contentLength) => this.ContentLength = contentLength;
    public WebClientRequestBody CreateBody() => new WebClientRequestBody(
        this.ContentLength, SinkTrueContentType ?? "application/octet-stream", this.TextEncoding, OnStreamRequested);
}

public abstract class WebClientService : IService, IDisposable
{
    public WebClientService()
    {
        this.CurrentSockets = SingletonResourceRepository<byte, SocketsHttpHandler>.Get().Take(0, HttpSocketsFactory);
        this.CurrentClient = SingletonResourceRepository<byte, HttpClient>.Get().Take(0, HttpClientFactory);
    }

    private SocketsHttpHandler HttpSocketsFactory(byte arg) => new()
    {
        AutomaticDecompression =
            DecompressionMethods.GZip | DecompressionMethods.Deflate | DecompressionMethods.Brotli,
    };

    private HttpClient HttpClientFactory(byte arg) => new HttpClient(this.CurrentSockets.Instance, false);

    protected abstract WebClientRequestHead GetRequestHead(StampedMap constants, IInteraction interaction);

    protected abstract WebClientRequestBody? GetRequestBody(StampedMap constants, IInteraction interaction,
        Stream stream);

    [EventOccasion("Source response body here; has status code in register")]
    public event CallForInteraction? OnThen;

    [NeverHappens] public event CallForInteraction? OnElse;

    [EventOccasion("Likely when the request body was wrong; check message.")]
    public event CallForInteraction? OnException;

    public void Enter(StampedMap constants, IInteraction interaction)
    {
        var head = GetRequestHead(constants, interaction);
        var body = GetRequestBody(constants, interaction);
        var request = BuildRequestMessage(head, body);
        CurrentClient.Instance.
        using var response = CurrentClient.Instance.Send(request, HttpCompletionOption.ResponseHeadersRead);
        OnThen?.Invoke(this, new HttpResponseSourcingInteraction(interaction, response));
    }

    private System.Net.Http.HttpMethod[] MethodsWithBodies =
    [
        System.Net.Http.HttpMethod.Post, System.Net.Http.HttpMethod.Put, System.Net.Http.HttpMethod.Delete,
        System.Net.Http.HttpMethod.Patch
    ];

    private readonly (Guid Guid, HttpClient Instance) CurrentClient;
    private readonly (Guid Guid, SocketsHttpHandler Instance) CurrentSockets;

    private HttpRequestMessage BuildRequestMessage(WebClientRequestHead head, WebClientRequestBody? body) =>
        head.ApplyModifications(IsRequestWithoutBody(head, ref body)
            ? new HttpRequestMessage(head.Method, head.URL)
            : new HttpRequestMessage(head.Method, head.URL) { Content = BuildRequestContent(body), });

    private bool IsRequestWithoutBody(WebClientRequestHead head, [NotNullWhen(false)] ref WebClientRequestBody? body) =>
        !MethodsWithBodies.Contains(head.Method) || body == null || body.ContentLength == 0;

    private static StreamContent BuildRequestContent(WebClientRequestBody body)
    {
        var content = new StreamContent(body.Data);
        var mediaTypeHeader = new MediaTypeHeaderValue(body.ContentType);
        if (body.ContentEncoding is not NoEncoding)
            mediaTypeHeader.CharSet = body.ContentEncoding.WebName;
        content.Headers.ContentType = mediaTypeHeader;
        if (body.ContentLength > 0)
            content.Headers.ContentLength = body.ContentLength;
        return content;
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);

    public void Dispose()
    {
        SingletonResourceRepository<byte, SocketsHttpHandler>.Get().Return(0, CurrentSockets.Guid);
        SingletonResourceRepository<byte, SocketsHttpHandler>.Get().Return(0, CurrentClient.Guid);
    }
}