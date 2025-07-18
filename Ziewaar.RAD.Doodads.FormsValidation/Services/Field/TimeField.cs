namespace Ziewaar.RAD.Doodads.FormsValidation.Services.Field;
#pragma warning disable 67
[Category("HTTP Forms")]
[Title("Time Field Validation")]
[Description("Use in conjunction with ValidateForm to validate Time values.")]
public class TimeField : ValidatingField<TimeOnly>
{
    private CultureInfo? ConfiguredCulture = null;
    private TimeOnly Earliest;
    private TimeOnly Latest;
    protected override void SetLowerBoundary(object? boundary) =>
        this.Earliest = TimeOnly.TryParse(boundary?.ToString() ?? "09:00", CultureInfo.InvariantCulture, out var only)
            ? only
            : new TimeOnly(09, 00, 00);
    protected override void SetUpperBoundary(object? boundary)=>
        this.Latest = TimeOnly.TryParse(boundary?.ToString() ?? "17:00", CultureInfo.InvariantCulture, out var only)
            ? only
            : new TimeOnly(17, 00, 00);
    protected override void SetPrimaryConstraint(object? rangeCandidate)
    {
        try
        {
            ConfiguredCulture = CultureInfo.GetCultureInfo(rangeCandidate?.ToString() ?? "en-US");
        }
        catch (Exception)
        {
            ConfiguredCulture = CultureInfo.InvariantCulture;
        }
    }
    protected override bool TryValidate(StampedMap constants, string valueToValidate,
        [NotNullWhen(true)] out object? validatedValue)
    {
        if (TimeOnly.TryParse(valueToValidate, ConfiguredCulture ?? CultureInfo.InvariantCulture, out var dt) &&
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