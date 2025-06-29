namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
public class CookieRepository
{
    private readonly SortedSet<ComponentCookie> _backingStore = new();
    public ComponentCookie Create()
    {
        ComponentCookie newCookie;
        do
        {
            newCookie = ComponentCookie.CreateNew();
        } while (_backingStore.Contains(newCookie));
        _backingStore.Add(newCookie);
        return newCookie;
    }
    public void Revoke(ComponentCookie cookie) => _backingStore.Remove(cookie);
    public bool IsValid(ComponentCookie cookie) => _backingStore.Contains(cookie);
    public ComponentCookie ValidateOrReplace(ComponentCookie cookie) => IsValid(cookie) ? cookie : Create();
}