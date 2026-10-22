namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;
#pragma warning disable 67
public class CsrfTokenSourceInteraction(IInteraction parent, CsrfFields fields, string rawCookieValue) : ICsrfTokenSourceInteraction
{
    public static CsrfTokenSourceInteraction CreateFor(IInteraction interaction, string csrfCookieValue) =>
        new(interaction, CsrfTokenRepository.Instance.RecoverForCookie(csrfCookieValue), csrfCookieValue);
    public string RawCsrfCookie => rawCookieValue;
    public ICsrfFields Fields => fields;
    public IInteraction Stack => parent;
    public object Register => parent.Register;
    public IReadOnlyDictionary<string, object> Memory => parent.Memory;
}