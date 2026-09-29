using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvSetOverlayImage : MpvService
{
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForCustom(interaction, out MpvOverlayLayerSpec layerSpec);
        BasicException.ForCustom(interaction, out MpvOverlayRectangleSpec rectSpec);
        var file = FileFromPrimaryOrRegister(constants, interaction);
        player.Overlay.SetOverlayPicture(layerSpec.layerNumber, file, rectSpec.x, rectSpec.y, rectSpec.w, rectSpec.h);
    }
}