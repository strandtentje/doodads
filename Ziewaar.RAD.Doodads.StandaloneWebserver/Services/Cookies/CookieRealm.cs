namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
public class CookieRealm : IService
{
    private readonly UpdatingPrimaryValue RealmName = new();
    private string? CurrentRealm;
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, RealmName).IsRereadRequired(out string? newRealm) &&
            newRealm != null)
            this.CurrentRealm = newRealm;
        if (string.IsNullOrWhiteSpace(this.CurrentRealm))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "Cookie realm must be explicitly named"));
            return;
        }
        var cookieRepository = CookieRealmRepository.Instance.Open(CurrentRealm);
        OnThen?.Invoke(this, new CookieRealmInteraction(interaction, cookieRepository));
    }
    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}