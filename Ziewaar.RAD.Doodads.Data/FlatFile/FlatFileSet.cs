using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CoreLibrary;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

[Category("Databases & Querying")]
[Title("Assign a flatfile key")]
[Description("""
    Provided an open flatfile, set the member with the key provided in register. Provide the 
    desired members as named constants with their defaults. Then assign that key key
    with values available in memory, otherwise the defaults.
    """)]
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
