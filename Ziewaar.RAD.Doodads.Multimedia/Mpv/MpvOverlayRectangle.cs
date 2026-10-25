using LibMpvWrapper;
using Ziewaar.RAD.Doodads.CoreLibrary.Data;
using Ziewaar.RAD.Doodads.CoreLibrary.Documentation;
using Ziewaar.RAD.Doodads.CoreLibrary.ExtensionMethods;
using Ziewaar.RAD.Doodads.CoreLibrary.Interfaces;
using Ziewaar.RAD.Doodads.CoreLibrary.Predefined;
using Ziewaar.RAD.Doodads.Multimedia.Mpv.Interaction;

namespace Ziewaar.RAD.Doodads.Multimedia.Mpv;

[Category("Multimedia")]
[Title("Have MPV overlay rectangle")]
[Description("""
             Requires an MpvInstance, defines a rectangle to put MpvSetOverlayImage into
             """)]
public class MpvOverlayRectangle : MpvService
{
    [EventOccasion("Use MpvSetOverlayImage here to set the image in the rectange")]
    public override event CallForInteraction? OnThen;
    [PrimarySetting("Define a rectangle here as `[xposition],[yposition] [width]x[height]` (may also be in register)")]
    private readonly UpdatingPrimaryValue OverlayRectangle = new UpdatingPrimaryValue();
    protected override void TryEnter(StampedMap constants, IInteraction interaction, MpvPlayer player)
    {
        var parts = PrimaryAndRegisterParts(constants, interaction, ' ', 'x', ',', ':');
        BasicException.ForDequeue(parts, "4 parts expected in primary or register specifying x,y,w,h - x was missing",
            out string xString);
        BasicException.ForDequeue(parts, "4 parts expected in primary or register specifying x,y,w,h - y was missing",
            out string yString);
        BasicException.ForDequeue(parts, "4 parts expected in primary or register specifying x,y,w,h - w was missing",
            out string wString);
        BasicException.ForDequeue(parts, "4 parts expected in primary or register specifying x,y,w,h - h was missing",
            out string hString);
        BasicException.ForConstraint(xString, -3840, 7680, "x position out of range (-3840 <-> 7680)",
            out int xPosition);
        BasicException.ForConstraint(yString, -2160, 4320, "y position out of range (-2160 <-> 4320)",
            out int yPosition);
        BasicException.ForConstraint(wString, 1, 7680, "width out of range (0 <-> 7680)",
            out int width);
        BasicException.ForConstraint(hString, 1, 7680, "height out of range (0 <-> 4320)",
            out int height);
        OnThen?.Invoke(this,
            interaction.AppendCustom(new MpvOverlayRectangleSpec(xPosition, yPosition, width, height)));
    }
}