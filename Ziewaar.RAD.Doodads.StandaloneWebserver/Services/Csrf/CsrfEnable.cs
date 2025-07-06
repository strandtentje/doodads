#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;

[Category("Http")]
[Title("Enable CSRF Field Obfuscation")]
[Description("""
             For the services that support it, initializes a system that turns plaintext
             field names into obfuscated field names that may only be used once.
             """)]
public class CsrfEnable : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        CookieRealm realm = new();
        SessionCookie session = new();

        void RealmSelected(object sender, IInteraction realmInteraction)
        {
            realm.OnThen -= RealmSelected;
            session.Enter(new StampedMap("CSRF-Anchor"), realmInteraction);
        }
        void CookieSelected(object sender, IInteraction cookieInteraction)
        {
            session.OnThen -= CookieSelected;
            if (cookieInteraction is not RetrievedCookieInteraction csrfCookieInteraction ||
                csrfCookieInteraction.CookieName != "CSRF-Anchor" ||
                csrfCookieInteraction.Register is not string csrfCookieText)
            {
                OnException?.Invoke(this,
                    new CommonInteraction(cookieInteraction,
                        "Got a cookie registration but it wasnt for the CSRF anchor"));
                return;
            }
            OnThen?.Invoke(this, CsrfTokenSourceInteraction.CreateFor(interaction, csrfCookieText));
        }

        realm.OnThen += RealmSelected;
        session.OnThen += CookieSelected;

        realm.Enter(new StampedMap("CSRF-Anchor"), interaction);
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}