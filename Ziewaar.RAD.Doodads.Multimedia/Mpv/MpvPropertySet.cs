using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvPropertySet : MpvService
{
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForNullOrEmpty(this.Primary(constants), "property name req'd in primary constant",
            out var propertyName);
        BasicException.ForNullOrEmpty(this.Register(interaction), "property value req'd in value",
            out var propertyValue);
        player[propertyName] = propertyValue;
    }
}