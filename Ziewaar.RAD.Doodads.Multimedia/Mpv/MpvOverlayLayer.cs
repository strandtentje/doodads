using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvOverlayLayer : MpvService
{
    public override event CallForInteraction? OnThen;

    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForConstraint(PrimaryOrRegister(constants, interaction), 0, 63,
            "layer number expected in primary or register, must be between 0 and 64", out int layerNumber);
        OnThen?.Invoke(this, interaction.AppendCustom(new MpvOverlayLayerSpec(layerNumber)));
    }
}