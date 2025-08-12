namespace Ziewaar.RAD.Doodads.Python;
public class PythonEnvironmentInteraction(IInteraction interaction, IPythonEnvironment environment) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public IPythonEnvironment Environment => environment;
}