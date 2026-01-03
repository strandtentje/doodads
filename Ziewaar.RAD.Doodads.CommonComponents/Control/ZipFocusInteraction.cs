using System.Collections.Concurrent;

namespace Ziewaar.RAD.Doodads.CommonComponents.Control;

public class ZipFocusInteraction(IInteraction interaction, BlockingCollection<IInteraction> bc) : IInteraction
{
    public IInteraction Stack => interaction;
    public object Register => interaction.Register;
    public IReadOnlyDictionary<string, object> Memory => interaction.Memory;
    public readonly BlockingCollection<IInteraction> BlockingCollection = bc;
}