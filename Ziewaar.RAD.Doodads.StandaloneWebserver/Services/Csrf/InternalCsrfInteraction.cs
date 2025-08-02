namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.Csrf;
#pragma warning disable 67
internal class InternalCsrfInteraction(IInteraction interaction) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
}