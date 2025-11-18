namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
public class RetrievedCookieInteraction(
    IInteraction interaction,
    HttpHeadInteraction head,
    CookieRealmInteraction realm,
    ComponentCookie serverCookie,
    Cookie clientCookie) : IInteraction, ICookieInteraction
{
    public IInteraction Stack => interaction;
    public string CookieName => clientCookie.Name;
    public object Register => clientCookie.Value;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public bool TryRevoke()
    {
        if (head.Context.Response.OutputStream.Length > 0 ||
            head.Context.Response.OutputStream.Position > 0)
            return false;
        realm.Cookies.Revoke(serverCookie);
        clientCookie.Expires = DateTime.MinValue;
        head.Context.Response.SetCookie(clientCookie);
        return true;
    }
}