namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.FormBuilder;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Text Field Validation")]
[Description("""
             Use in conjunction with ValidateForm to validate text
             """)]
public class TextField : ValidatingField<string>
{
    private decimal LowerLimit;
    private decimal UpperLimit;
    protected override void SetLowerBoundary(object? boundary) =>
        boundary.TryConvertNumericToDecimal(0, out LowerLimit);
    protected override void SetUpperBoundary(object? boundary) =>
        boundary.TryConvertNumericToDecimal(32, out UpperLimit);
    protected override bool TryValidate(StampedMap constants, string valueToValidate,
        [NotNullWhen(true)] out object? validatedValue)
    {
        validatedValue = null;
        if (valueToValidate.Length <= UpperLimit && valueToValidate.Length >= LowerLimit)
        {
            validatedValue = valueToValidate;
            return true;
        }
        else
            return false;
    }
}