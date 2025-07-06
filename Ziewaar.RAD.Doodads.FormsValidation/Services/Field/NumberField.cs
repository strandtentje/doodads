namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.FormBuilder;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Number Field Validation")]
[Description("""
             Use in conjunction with ValidateForm to validate numeric values.
             """)]
public class NumberField : ValidatingField<decimal>
{
    private decimal LowerLimit;
    private decimal UpperLimit;
    protected override void SetLowerBoundary(object? boundary) =>
        boundary.TryConvertNumericToDecimal(0, out LowerLimit);
    protected override void SetUpperBoundary(object? boundary) =>
        boundary.TryConvertNumericToDecimal(1, out UpperLimit);
    protected override bool TryValidate(StampedMap constants, string valueToValidate,
        [NotNullWhen(true)] out object? validatedValue)
    {
        validatedValue = null;
        if (!decimal.TryParse(valueToValidate, CultureInfo.InvariantCulture, out decimal candidateNumber))
            return false;
        if (candidateNumber < this.LowerLimit || candidateNumber > this.UpperLimit)
            return false;
        validatedValue = candidateNumber;
        return true;
    }
}