namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Cookies;
public class CookieRealmInteraction(IInteraction interaction, CookieRepository cookieRepository) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public CookieRepository Cookies => cookieRepository;
}