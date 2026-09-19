using Define.Doodads.Expo.Timeline;
using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvCommand : MpvService
{
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForPopulatedArray(this.PrimaryParts(constants), "at least command is required", out var items);
        player.SendCommand([.. items.Select(interaction.ReplaceWhenMemory)]);
    }
}