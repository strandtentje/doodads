#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
[Category("Http & Routing")]
[Title("Revoke the cookie in this realm")]
[Description("""
             If theres a cookie for this realm, revoke it.
             """)]
public class RevokeCookie : IService
{
    [EventOccasion("When the cookie was revoked")]
    public event CallForInteraction? OnThen;
    [NeverHappens]
    public event CallForInteraction? OnElse;
    [EventOccasion("Either there was no cookie or we can't change the headers anymore.")]
    public event CallForInteraction? OnException;
    public void Enter(StampedMap constants, IInteraction interaction)
    {
        if (interaction.TryGetClosest<RetrievedCookieInteraction>(out var cookieInteraction) && 
            cookieInteraction?.TryRevoke() == true)
        {
            OnThen?.Invoke(this, interaction);
        }
        else
        {
            OnException?.Invoke(this, new CommonInteraction(
                interaction, "cannot revoke before a session cookie was found, or after content was written."));
        }
    }
    public void HandleFatal(IInteraction source, Exception ex) =>
        OnException?.Invoke(this, source);
}