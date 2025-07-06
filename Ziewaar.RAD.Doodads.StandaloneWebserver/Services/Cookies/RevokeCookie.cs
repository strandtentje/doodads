#pragma warning disable 67
namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
[Category("Http")]
[Title("Revoke the cookie in this realm")]
[Description("""
             If theres a cookie for this realm, revoke it.
             """)]
public class RevokeCookie : IService
{
    public event CallForInteraction? OnThen;
    public event CallForInteraction? OnElse;
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