using Define.Doodads.Expo.Timeline;
using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Multimedia;

public abstract class MpvService : BasicService
{
    public override void TryEnter(StampedMap constants, IInteraction interaction)
    {
        if (!interaction.TryGetCustom<MpvPlayer>(out var player) || player == null)
            throw new BasicException("MpvPlayer required for this");
        TryEnter(constants, interaction, player);
    }

    protected abstract void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player);
}