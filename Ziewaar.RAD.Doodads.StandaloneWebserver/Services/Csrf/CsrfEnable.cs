#pragma warning disable 67
using System.ComponentModel;
using Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
[Category("HTTP")]
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