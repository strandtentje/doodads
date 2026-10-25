using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.FileMetadata.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.FileMetadata;

[Category("Multimedia")]
[Title("Iterate genres of multimedia file")]
[Description("""
             Requires AtomicMetadata to have run previously. Will iterate through genres in Register OnThen
             """)]
public class GenreMetadata : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out FileMetadataInteraction fmi);
        RepeatToRegister(constants, interaction, fmi.Tag.Genres);
    }
}