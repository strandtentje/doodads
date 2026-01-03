using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class ZipInteraction(IInteraction interaction, List<(string, BlockingCollection<IInteraction>)> mergeCollections) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public readonly List<(string Name, BlockingCollection<IInteraction> Collection)> MergeCollections = mergeCollections;
}