using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Send command to mpv")]
[Description("""
             Requires an MpvInstance, allows you to send commands (check docs on mpv.io)
             """)]
public class MpvCommand : MpvService
{
    [PrimarySetting("""
                    One or multiple command parts. Put memory names between {curly brackets} to get command 
                    arguments from memory.
                    """)]
    private readonly UpdatingPrimaryValue CommandPartsConstant = new UpdatingPrimaryValue();
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForPopulatedArray(this.PrimaryParts(constants), "at least command is required", out var items);
        player.SendCommand([.. items.Select(interaction.ReplaceWhenMemory)]);
    }
}