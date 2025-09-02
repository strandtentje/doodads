using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.Bounding;
public class BoundsValidatingWeekCollection(DateOnly lbound, DateOnly ubound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        if (value is DateOnly existingWeek && existingWeek >= lbound && existingWeek <= ubound)
            BackingValues.Add(transformed = existingWeek);
        else if (WeekOnly.TryParse(value.ToString() ?? "", out var parsedWeek) &&
                 parsedWeek.ToDateOnly() >= lbound && parsedWeek.ToDateOnly() <= ubound)
            BackingValues.Add(transformed =parsedWeek.ToDateOnly());
        else
        {
            Reason = "Bad format or out of bounds";
            IsSatisfied = false;
        }
    }
}