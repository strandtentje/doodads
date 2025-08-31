using Ziewaar.RAD.Doodads.FormsValidation.Services.Support;

namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
public class ValidatingWeekCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        if (value is DateOnly existingWeek)
        {
            BackingValues.Add(transformed = existingWeek);
        }
        else if (WeekOnly.TryParse(value.ToString() ?? "", out var parsedWeek))
        {
            BackingValues.Add(transformed = parsedWeek.ToDateOnly());
        }
        else
            IsSatisfied = false;
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}