#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;

[Category("Http")]
[Title("Define Realm of Cookie")]
[Description("""
             Within a CookieRealm, either find out if its a known cookie, or create one.
             Puts the cookie value in the body.
             """)]
public class SessionCookie : IService
{
    [PrimarySetting("Name of the cookie as it goes over HTTP")]
    private readonly UpdatingPrimaryValue CookieNameConst = new();
    [NamedSetting("domain", "Domain to which the cookie is restricted in the response")]
    private readonly UpdatingKeyValue CookieDomainConst = new("domain");
    [NamedSetting("path", "Path to which the cookie is restricted in the response")]
    private readonly UpdatingKeyValue CookiePathConst = new("path");
    [NamedSetting("expires", "Cookie lifetime in whole or decimal hours")]
    private readonly UpdatingKeyValue CookieLifetimeConst = new("expires");
    private string? CurrentCookieName, CurrentCookieDomain, CurrentCookiePath;
    private decimal CurrentCookieLifetimeHours;
    [EventOccasion("Continues here with a cookie")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Likely when misconfigured; read the error.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if ((constants, CookieNameConst).IsRereadRequired(out string? newCookieName) &&
            newCookieName != null)
            this.CurrentCookieName = newCookieName;
        if ((constants, CookieDomainConst).IsRereadRequired(out string? newCookieDomain) &&
            newCookieDomain != null)
            this.CurrentCookieDomain = newCookieDomain;
        if ((constants, CookiePathConst).IsRereadRequired(() => "/", out string? newCookiePath) &&
            newCookiePath != null)
            this.CurrentCookiePath = newCookiePath;
        if ((constants, CookieLifetimeConst).IsRereadRequired(() => 48M, out var newCookieLife))
            this.CurrentCookieLifetimeHours = newCookieLife;
        if (string.IsNullOrWhiteSpace(this.CurrentCookieName) ||
            string.IsNullOrWhiteSpace(this.CurrentCookiePath))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "cookie name and path is required"));
            return;
        }
        if (this.CurrentCookieLifetimeHours == 0)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "cookie lifetime cannot be 0 hours"));
            return;
        }
        if (!this.CurrentCookieName.All(char.IsAsciiLetterOrDigit))
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "cookie name is invalid"));
            return;
        }
        if (!interaction.TryGetClosest<HttpHeadInteraction>(out var head) ||
            head == null)
        {
            OnException?.Invoke(this, new CommonInteraction(interaction, "can only set cookie on http request"));
            return;
        }
        if (!interaction.TryGetClosest<CookieRealmInteraction>(out var realm) ||
            realm == null)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "can only set cookie when CookieRealm is defined"));
            return;
        }
        if (head.Context.Response.OutputStream.Position > 0 ||
            head.Context.Response.OutputStream.Length > 0)
        {
            OnException?.Invoke(this,
                new CommonInteraction(interaction, "can only set cookie on request that doesn't have a body yet"));
            return;
        }
        ProceedToCook(interaction, head, realm);
    }
    private void ProceedToCook(IInteraction interaction, HttpHeadInteraction head, CookieRealmInteraction realm)
    {
        var cookieWorkingSet = head.Context.Request.Cookies.Where(x => x.Name == this.CurrentCookieName! &&
                                                                       x.Expired == false).ToArray();
        if (cookieWorkingSet.Length > 1)
        {
            foreach (var excessiveCookie in cookieWorkingSet)
            {
                if (ComponentCookie.TryParse(excessiveCookie.Value, out var validIncomingCookie))
                {
                    realm.Cookies.Revoke(validIncomingCookie);
                }
                excessiveCookie.Expires = DateTime.MinValue;
                head.Context.Response.SetCookie(excessiveCookie);
            }
            cookieWorkingSet = [];
        }

        if (cookieWorkingSet.Length == 0)
        {
            cookieWorkingSet =
            [
                new Cookie(CurrentCookieName!, ComponentCookie.CreateNew().ToString())
                {
                    Path = "/",
                    Expires = DateTime.Now,
                }
            ];
            if (!string.IsNullOrWhiteSpace(CurrentCookieDomain))
                cookieWorkingSet[0].Domain = CurrentCookieDomain;
        }

        if (cookieWorkingSet.Length == 1 && ComponentCookie.TryParse(cookieWorkingSet[0].Value, out var cookie))
        {
            var goodCookie = realm.Cookies.ValidateOrReplace(cookie);
            cookieWorkingSet[0].Value = goodCookie.ToString();
            cookieWorkingSet[0].Expires += TimeSpan.FromHours((double)CurrentCookieLifetimeHours);
            head.Context.Response.SetCookie(cookieWorkingSet[0]);
            OnThen?.Invoke(this, new RetrievedCookieInteraction(interaction, head, realm, cookie, cookieWorkingSet[0]));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}