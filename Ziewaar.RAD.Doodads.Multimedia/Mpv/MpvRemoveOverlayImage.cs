using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.Mpv.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Remove MPV overlay picture")]
[Description("""
             Requires MpvInstance, and MpvOverlayLayer. Will remove that overlay.
             """)]
public class MpvRemoveOverlayImage : MpvService
{
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForCustom(interaction, out MpvOverlayLayerSpec layerSpec);
        player.Overlay.RemoveOverlay(layerSpec.layerNumber);
    }
}