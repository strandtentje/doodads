namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
public class CookieRealmRepository
{
    private CookieRealmRepository() { }
    public static readonly CookieRealmRepository Instance = new();
    private readonly SortedList<string, CookieRepository> Realms = new SortedList<string, CookieRepository>();
    public CookieRepository Open(string name)
    {
        if (!Realms.TryGetValue(name, out var foundRealm))
            Realms[name] = foundRealm = new();
        return foundRealm;
    }
}