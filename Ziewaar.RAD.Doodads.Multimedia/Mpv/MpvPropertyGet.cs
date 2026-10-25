using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Get MPV property value")]
[Description("""
             As title describes, gets property value. Name of property in primary constant.
             """)]
public class MpvPropertyGet : MpvService
{
    [EventOccasion("Property value comes out here in register")]
    public override event CallForInteraction? OnThen;
    [PrimarySetting("Name of property to retrieve")]    
    private readonly UpdatingPrimaryValue ProperyNameConstant = new UpdatingPrimaryValue();
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForNullOrEmpty(this.Primary(constants), "property name req'd in primary constant",
            out var propertyName);
        OnThen?.Invoke(this, interaction.AppendRegister(player[propertyName] ?? ""));
    }
}