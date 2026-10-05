using System.Linq;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.MemoryStore;

public class MemoryStoreSelect : MemoryStore
{
    public override event CallForInteraction? OnThen;
    public override event CallForInteraction? OnElse;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        var items = PrimaryPartsFromMemory(constants, interaction).ToArray();
        FlatFileMemberMemory ourStore;
        bool isNew = false;
        lock (StoreLock)
        {
            if (!Store.TryGetValue(items, out ourStore))
            {
                Store[items] = ourStore = new FlatFileMemberMemory(constants.NamedItems, new InteractionMirroringDictionary(interaction));
                isNew = true;
            }
        }
        if (!isNew)
            OnThen?.Invoke(this, interaction.AppendMemory(new FlatFileMemberMemory(constants.NamedItems, ourStore)));
        else
            OnElse?.Invoke(this, interaction.AppendMemory(new FlatFileMemberMemory(constants.NamedItems, ourStore)));
    }
}
