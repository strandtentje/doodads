using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CommonComponents.Filesystem;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

[Category("Databases & Querying")]
[Title("Clear a flatfile")]
[Description("""
    Provided an open flatfile, clears it contents
    """)]
public class FlatFileClear : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetCustom<JsonFile>(out var flatFile) || flatFile == null)
            throw new BasicException("no flat file in scope");
        if (interaction.Register.ToString() is not string flatFileKey)
            throw new BasicException("flat file key expected");
        if (constants.PrimaryConstant.IsntJustAnObject() && constants.PrimaryConstant.ToString() is string memberKey)
        {
            flatFile.Remove(flatFileKey, memberKey);
        } else
        {
            flatFile.Remove(flatFileKey);
        }
    }
}
