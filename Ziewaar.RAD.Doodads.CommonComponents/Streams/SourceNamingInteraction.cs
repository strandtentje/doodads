namespace Ziewaar.RAD.Doodads.Cryptography;

public class SourceNamingInteraction(
    IInteraction interaction,
    string currentSourceName,
    ISourcingInteraction sourcingInteraction) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public string SourceName = currentSourceName;
    public ISourcingInteraction SourcingInteraction => sourcingInteraction;
}