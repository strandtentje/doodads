using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

public class FlatFileSet : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetCustom<JsonFile>(out var flatFile) || flatFile == null)
            throw new BasicException("no flat file in scope");
        if (interaction.Register.ToString() is not string flatFileKey)
            throw new BasicException("flat file key expected");
        var imd = new InteractionMirroringDictionary(interaction);
        flatFile.Set(flatFileKey, new FlatFileMemberMemory(constants.NamedItems, imd));
    }
}
