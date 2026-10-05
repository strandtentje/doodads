using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Data.MemoryStore;

public class MemoryStoreDelete : MemoryStore
{
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var items = PrimaryPartsFromMemory(constants, interaction).ToArray();
        bool isRemoved = false;
        lock (StoreLock)
        {
            isRemoved = Store.Remove(items);
        }
        if (isRemoved)
            OnThen?.Invoke(this, interaction);
        else
            OnElse?.Invoke(this, interaction);
    }
}
