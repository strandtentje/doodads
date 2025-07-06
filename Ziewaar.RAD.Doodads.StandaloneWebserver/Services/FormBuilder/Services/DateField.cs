namespace Ziewaar.RAD.Doodads.StandaloneWebserver.Services.FormBuilder;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Date Field Validation")]
[Description("""
             Use in conjunction with ValidateForm to validate date values.
             """)]
public class DateField : ValidatingField<DateOnly>
{
    private CultureInfo? ConfiguredCulture;
    private DateOnly Earliest;
    private DateOnly Latest;
    protected override void SetLowerBoundary(object? boundary) =>
        this.Earliest = DateOnly.TryParse(boundary?.ToString() ?? "2000-01-01", CultureInfo.InvariantCulture, out var only)
            ? only
            : new DateOnly(2000, 01, 01);
    protected override void SetUpperBoundary(object? boundary)=>
        this.Latest = DateOnly.TryParse(boundary?.ToString() ?? "2000-12-31", CultureInfo.InvariantCulture, out var only)
            ? only
            : new DateOnly(2000, 12, 31);
    protected override void SetPrimaryConstraint(object? rangeCandidate)
    {
        try
        {
            ConfiguredCulture = CultureInfo.GetCultureInfo(rangeCandidate?.ToString() ?? "en-GB");
        }
        catch (Exception)
        {
            ConfiguredCulture = CultureInfo.InvariantCulture;
        }
    }
    protected override bool TryValidate(
        StampedMap constants,
        string valueToValidate,
        [NotNullWhen(true)] out object? validatedValue)
    {
        if (DateOnly.TryParse(valueToValidate, ConfiguredCulture ?? CultureInfo.InvariantCulture, out var dt) &&
            dt >= this.Earliest && dt <= this.Latest)
        {
            validatedValue = dt;
            return true;
        }
        else
        {
            validatedValue = null;
            return false;
        }
    }
}