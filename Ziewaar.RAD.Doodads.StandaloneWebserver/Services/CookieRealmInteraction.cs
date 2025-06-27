namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services;
public class CookieRealmInteraction(IInteraction interaction, CookieRepository cookieRepository) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public CookieRepository Cookies => cookieRepository;
}