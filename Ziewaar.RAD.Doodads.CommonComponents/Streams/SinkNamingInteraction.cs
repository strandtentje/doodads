namespace Ziewaar.RAD.Doodads.CommonComponents.Streams;

public class SinkNamingInteraction(
    IInteraction interaction,
    string currentSinkName,
    ISinkingInteraction sinkingInteraction) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public string SinkName = currentSinkName;
    public ISinkingInteraction SinkingInteraction => sinkingInteraction;
}