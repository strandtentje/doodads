using System.Net.Http.Headers;
using Ziewaar.RAD.Doodads.CommonComponents.TextTemplating;

namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Client;
#pragma warning disable 67

[Category("Http & Routing")]
[Title("Send HTTP Request")]
[Description("""
             For doing API calls, web requests, AJAX (but not really)
             """)]
public class HttpSend : IService, IDisposable
{
    private readonly HttpClientPoolAccessor PooledClient = new();
    private HttpClient Client => PooledClient.Instance;

    [PrimarySetting("HTTP Method to Use; will default to GET")]
    private readonly UpdatingPrimaryValue MethodNameConst = new UpdatingPrimaryValue();

    [NamedSetting("body", "Content type of request body; defaults to application/json")]
    private readonly UpdatingKeyValue RequestBodyTypeConstant = new UpdatingKeyValue("body");

    [NamedSetting("encoding", "Encoding of request body; defaults to UTF8")]
    private readonly UpdatingKeyValue RequestEncodingConstant = new UpdatingKeyValue("encoding");

    private string CurrentMethodName = "GET";
    private System.Net.Http.HttpMethod? CurrentMethod = System.Net.Http.HttpMethod.Get;
    private string CurrentRequestBodyType = "application/json";
    private Encoding CurrentRequestEncoding = Encoding.UTF8;

    [EventOccasion("When URL is needed")]
    public event CallForInteraction? SinkUrl;

    [EventOccasion("When request body is needed")]
    public event CallForInteraction? SinkBody;

    [EventOccasion("When 2xx status")]
    public event CallForInteraction? OnThen;
    [EventOccasion("When non-2xx status")]
    public event CallForInteraction? OnElse;
    [EventOccasion("When something else still went wrong")]
    public event CallForInteraction? OnException;
    [EventOccasion("When the HTTP request failed on the network layer")]
    public event CallForInteraction? OnNetworkError;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, MethodNameConst).IsRereadRequired(out string? methodName))
            this.CurrentMethodName = methodName ?? "GET";
        this.CurrentMethod = System.Net.Http.HttpMethod.Parse(this.CurrentMethodName);
        if ((constants, RequestBodyTypeConstant).IsRereadRequired(out string? requestBodyType))
            this.CurrentRequestBodyType = requestBodyType ?? "application/octet-stream";
        if ((constants, RequestEncodingConstant).IsRereadRequired(out string? requestEncoding))
            this.CurrentRequestEncoding = requestEncoding is "binary" or null
                ? NoEncoding.Instance
                : Encoding.GetEncoding(requestEncoding);

        var urlSink = new TextSinkingInteraction(interaction);
        SinkUrl?.Invoke(this, urlSink);
        var urlText = urlSink.ReadAllText();
        var content = new PushStreamContent((str, _) =>
        {
            var bodySink = new RequestBodySinkingInteraction(interaction, CurrentRequestEncoding, str);
            SinkBody?.Invoke(this, bodySink);
        });
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(this.CurrentRequestBodyType);
        var message = new HttpRequestMessage(this.CurrentMethod, urlText) { Content = content, };
        HttpResponseMessage response;
        try
        {
            response = Client.Send(message);
        } catch(HttpRequestException reqex)
        {
            OnNetworkError?.Invoke(this, new CommonInteraction(interaction, reqex.Message));
            return;
        }
        if (response.IsSuccessStatusCode)
            OnThen?.Invoke(this, new HttpResponseSourcingInteraction(interaction, response));
        else
            OnElse?.Invoke(this, new HttpResponseSourcingInteraction(interaction, response));
    }

    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
    public void Dispose() => PooledClient.Dispose();
}