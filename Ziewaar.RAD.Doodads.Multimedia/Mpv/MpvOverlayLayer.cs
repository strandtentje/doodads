using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.Mpv.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Open MPV overlay number")]
[Description("""
             Requires an MpvInstance, brings an MPV overlay layer in scope.
             """)]
public class MpvOverlayLayer : MpvService
{
    [EventOccasion("Apply layer changes here using MpvOverlayRectange, MpvSetOverlayImage, MpvRemoveOverlayImage")]
    public override event CallForInteraction? OnThen;
    [PrimarySetting("Layer number to alter (may also come from register)")]
    private readonly UpdatingPrimaryValue LayerNumberConstant = new UpdatingPrimaryValue();
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForConstraint(PrimaryOrRegister(constants, interaction), 0, 63,
            "layer number expected in primary or register, must be between 0 and 64", out int layerNumber);
        OnThen?.Invoke(this, interaction.AppendCustom(new MpvOverlayLayerSpec(layerNumber)));
    }
}