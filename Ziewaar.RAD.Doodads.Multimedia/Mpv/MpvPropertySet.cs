using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Set MPV property value")]
[Description("""
             Requires MpvInstance, have property name in primary constant, have property value in register.
             """)]
public class MpvPropertySet : MpvService
{
    [PrimarySetting("Name of property")]
    private readonly UpdatingPrimaryValue PropertyName = new();
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForNullOrEmpty(this.Primary(constants), "property name req'd in primary constant",
            out var propertyName);
        BasicException.ForNullOrEmpty(this.Register(interaction), "property value req'd in value",
            out var propertyValue);
        player[propertyName] = propertyValue;
    }
}