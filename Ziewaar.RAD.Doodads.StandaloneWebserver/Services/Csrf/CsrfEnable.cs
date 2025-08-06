#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;
[Category("Http & Routing")]
[Title("Enable CSRF Field Obfuscation")]
[Description("""
             For the services that support it, initializes a system that turns plaintext
             field names into obfuscated field names that may only be used once.
             """)]
public class CsrfEnable : IService
{
    private readonly CookieRealm Realm = new();
    private readonly SessionCookie Session = new();
    [EventOccasion("Forms from here on out will be CSRF hardened")]
    public event CallForInteraction? OnThen;
    [NeverHappens] public event CallForInteraction? OnElse;
    [EventOccasion("Likely when CSRF was already enabled")]
    public event CallForInteraction? OnException;
    public CsrfEnable()
    {
        Realm.OnThen += RealmSelected;
        Session.OnThen += CookieSelected;
        Realm.OnException += (sender, interaction) => OnException?.Invoke(sender, interaction);
        Session.OnException += (sender, interaction) => OnException?.Invoke(sender, interaction);
    }
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.TryGetClosest(out InternalCsrfInteraction? _))
            OnException?.Invoke(this, new CommonInteraction(interaction, "CSRF already enabled"));
        else
            Realm.Enter(new StampedMap("CSRF-Anchor"), new InternalCsrfInteraction(interaction));
    }
    private void RealmSelected(object sender, IInteraction realmInteraction) =>
        Session.Enter(new StampedMap("CSRF-Anchor"), realmInteraction);
    private void CookieSelected(object sender, IInteraction cookieInteraction)
    {
        if (!cookieInteraction.IsCsrfCookie(out string? csrfCookieText))
            OnException?.Invoke(this, new CommonInteraction(cookieInteraction,
                "Got a cookie registration but it wasn't for the CSRF anchor"));
        else if (!cookieInteraction.TryGetClosest(out InternalCsrfInteraction? origin) || origin == null)
            OnException?.Invoke(this, new CommonInteraction(cookieInteraction,
                "Lost the internal csrf interaction"));
        else
            OnThen?.Invoke(this, CsrfTokenSourceInteraction.CreateFor(origin, csrfCookieText));
    }
    public void HandleFatal(IInteraction source, Exception ex) => OnException?.Invoke(this, source);
}