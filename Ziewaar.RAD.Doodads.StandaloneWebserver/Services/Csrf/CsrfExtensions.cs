namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;
#pragma warning disable 67
internal static class CsrfExtensions
{
    public static bool IsCsrfCookie(this IInteraction interaction, [NotNullWhen(true)] out string? cookieText)
    {
        if (interaction is RetrievedCookieInteraction
            {
                CookieName: "CSRF-Anchor", Register: string csrfCookieText
            })
        {
            cookieText = csrfCookieText;
            return true;
        }
        else
        {
            cookieText = null;
            return false;
        }
    }
}