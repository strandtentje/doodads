using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Data.FlatFile.Support;

namespace Ziewaar.RAD.Doodads.Data.FlatFile;

[Category("Databases & Querying")]
[Title("Isolate a flatfile key")]
[Description("""
    Provided an open flatfile, find the member with the key provided in register. Provide the 
    desired members as named constants with their defaults. If no item was found, defaults will
    be used entirely.
    """)]
public class FlatFileMembersOf : BasicService
{
    public override event CallForInteraction? OnThen;
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetCustom<JsonFile>(out var flatFile) || flatFile == null)
            throw new BasicException("no flat file in scope");
        if (interaction.Register.ToString() is not string flatFileKey)
            throw new BasicException("flat file key expected");
        var foundMember = flatFile.ForKey(flatFileKey);
        OnThen?.Invoke(this, interaction.AppendMemory(new FlatFileMemberMemory(constants.NamedItems, foundMember)));
    }
}
