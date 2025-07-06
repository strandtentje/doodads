namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.FormBuilder;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Reject Nested Validation")]
[Description("""
             Reject Nesting Validation Typically used in conjunction with one of the fields to 
             further validate a field before committing ie.
             NumberField("0-100", nest = true) {
                 OnInitial->DisplayField();
                 OnInitial->DisplayField();
                 OnInitial->DisplayField();
             }:SomeDataValidationService() {
                 OnBad->RejectValidation();
                 OnGood->AcceptValidation();
             };
             """)]
public class RejectValidation : ModifyValidation
{
    protected override bool IsValid => false;
}