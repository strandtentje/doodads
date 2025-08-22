namespace Ziewaar.RAD.Doodads.CommonComponents.Stdio;
public class DumpStopper(IInteraction interaction) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
}