using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvRemoveOverlayImage : MpvService
{
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForCustom(interaction, out MpvOverlayLayerSpec layerSpec);
        player.Overlay.RemoveOverlay(layerSpec.layerNumber);
    }
}