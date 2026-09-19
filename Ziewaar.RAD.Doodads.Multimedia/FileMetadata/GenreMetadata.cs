using Define.Doodads.Expo.Timeline;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class GenreMetadata : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        BasicException.ForInteraction(interaction, out FileMetadataInteraction fmi);
        RepeatToRegister(constants, interaction, fmi.Tag.Genres);
    }
}