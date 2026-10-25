using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.Mpv.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Set MPV overlay picture")]
[Description("""
             Requires MpvInstance, and MpvOverlayLayer. Takes file from Primary or Register to use as overlay picture.
             """)]
public class MpvSetOverlayImage : MpvService
{
    [PrimarySetting("Path to overlay picture. May also come from register.")]
    private readonly UpdatingPrimaryValue OverlayPictureConstant = new UpdatingPrimaryValue();
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForCustom(interaction, out MpvOverlayLayerSpec layerSpec);
        BasicException.ForCustom(interaction, out MpvOverlayRectangleSpec rectSpec);
        var file = FileFromPrimaryOrRegister(constants, interaction);
        player.Overlay.SetOverlayPicture(layerSpec.layerNumber, file, rectSpec.x, rectSpec.y, rectSpec.w, rectSpec.h);
    }
}