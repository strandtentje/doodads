using Define.Doodads.Expo.Timeline;
using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;

namespace Ziewaar.RAD.Doodads.Multimedia;

public class MpvPropertyGet : MpvService
{
    public override event CallForInteraction? OnThen;
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        BasicException.ForNullOrEmpty(this.Primary(constants), "property name req'd in primary constant",
            out var propertyName);
        OnThen?.Invoke(this, interaction.AppendRegister(player[propertyName]));
    }
}