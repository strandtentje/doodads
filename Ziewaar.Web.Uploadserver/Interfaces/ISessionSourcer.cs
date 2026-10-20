namespace Ziewaar.Web.Uploadserver.Interfaces;

public interface ISessionSourcer
{
    string GetSessionIdFor(HttpListenerRequest req);
}