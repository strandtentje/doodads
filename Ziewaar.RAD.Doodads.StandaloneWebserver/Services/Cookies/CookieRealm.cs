#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
[Category("Http")]
[Title("Define Realm of Cookie")]
[Description("""
    Scopes the cookie lookup, this is not the cookie name itself - required to use RevokeCookie and SessionCookie.
    """)]
public class CookieRealm : IService
{
    [PrimarySetting("Name of the cookie")]
    private readonly UpdatingPrimaryValue RealmName = new();
    private string? CurrentRealm;
    [EventOccasion("Continues with cookie name")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely happens when a name is missing")]
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