namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Interactions.Payloads;

public class HttpStatusCodeChange(HttpListenerResponse response)
{
    public HttpStatusCode StatusCode
    {
        get => (HttpStatusCode)response.StatusCode;
        set => response.StatusCode = (int)value;
    }
}
