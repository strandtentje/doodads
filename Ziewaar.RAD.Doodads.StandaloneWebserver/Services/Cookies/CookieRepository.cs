namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;

public class CookieRepository
{
    private readonly Lock StoreLock = new Lock();
    private readonly SortedSet<ComponentCookie> _backingStore = new();
    public ComponentCookie Create()
    {
        lock (StoreLock)
        {
            ComponentCookie newCookie;
            do
            {
                newCookie = ComponentCookie.CreateNew();
            } while (_backingStore.Contains(newCookie));
            _backingStore.Add(newCookie);
            return newCookie;
        }
    }
    public void Revoke(ComponentCookie cookie)
    {
        lock (StoreLock)
            _backingStore.Remove(cookie);
    }

    public bool IsValid(ComponentCookie cookie)
    {
        lock (StoreLock)
            return _backingStore.Contains(cookie);
    }

    public ComponentCookie ValidateOrReplace(ComponentCookie cookie) => IsValid(cookie) ? cookie : Create();
}