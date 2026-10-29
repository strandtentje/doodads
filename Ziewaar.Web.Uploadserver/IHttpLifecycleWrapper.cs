namespace Ziewaar.Web.Uploadserver;

public interface IHttpLifecycleWrapper
{
    string ActionBeforeHash { get; }
    string ActionAfterHash { get; }

    string UpperHttpMethod { get; }
    string SessionId { get; }
    Encoding? ResponseEncoding { get; set; }
    Stream ResponseStream { get; }
    Stream RequestStream { get; }
    string? RequestContentType { get; }
    long RequestContentLength { get; }

    void Bypass();
    string GetFromQueryString(string key);
    void SetCacheControl(string v);
    void SetKeepAlive(bool v);
    void TrySetResponseHeader(long contentLength, string contentType);
}
