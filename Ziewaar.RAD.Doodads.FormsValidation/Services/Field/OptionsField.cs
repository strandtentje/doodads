namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.FormBuilder;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Options Field Validation")]
[Description("""
             Use in conjunction with ValidateForm to validate comma-separated option strings
             """)]
public class OptionsField : ValidatingField<string[]>
{
    private decimal MinOptionCount;
    private decimal MaxOptionCount;
    private string[] ValidOptions = [];
    protected override void SetLowerBoundary(object? boundary) =>
        boundary.TryConvertNumericToDecimal(0, out MinOptionCount);
    protected override void SetUpperBoundary(object? boundary) =>
        boundary.TryConvertNumericToDecimal(1, out MaxOptionCount);
    protected override void SetPrimaryConstraint(object? rangeCandidate) => 
        this.ValidOptions = rangeCandidate?.ToString()?.Split(',', (StringSplitOptions)3) ?? [];
    protected override bool TryValidate(
        StampedMap constants, string valueToValidate, [NotNullWhen(true)] out object? validatedValue)
    {
        var split = valueToValidate.Split(',');
        if (split.Any(x => !this.ValidOptions.Contains(x)))
        {
            validatedValue = null;
            return false;
        }
        else
        {
            validatedValue = split;
            return true;
        }
    }
}