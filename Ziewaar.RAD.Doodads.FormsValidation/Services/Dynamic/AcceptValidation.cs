namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Dynamic;
#pragma warning disable 67

[Category("HTTP Forms")]
[Title("Accept Nesting Validation")]
[Description("""
             Reject Nesting Validation Typically used in conjunction with one of the fields to 
             further validate a field before committing ie.
             NumberField("0-100", nest = true) {
                 OnInitial->DisplayField();
                 OnValid->DisplayField();
                 OnInvalid->DisplayField();
             }:SomeDataValidationService() {
                 OnBad->RejectValidation();
                 OnGood->AcceptValidation();
             };
             """)]
public class AcceptValidation : ModifyValidation
{
    protected override bool GetValidity(StampedMap constants, IInteraction interaction) => true;
}
