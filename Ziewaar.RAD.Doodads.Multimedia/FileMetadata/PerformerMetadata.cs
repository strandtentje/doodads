using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.FileMetadata.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.FileMetadata;

[Category("Multimedia")]
[Title("Iterate performers of multimedia file")]
[Description("""
             Requires AtomicMetadata to have run previously. Will iterate through performer names in Register OnThen
             """)]
public class PerformerMetadata : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out FileMetadataInteraction fmi);
        RepeatToRegister(constants, interaction, fmi.Tag.Performers);
    }
}