namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class BoundsValidatingWeekCollection(DateOnly lbound, DateOnly ubound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value)
    {
        if (!IsSatisfied) return;
        if (value is DateOnly existingWeek && existingWeek >= lbound && existingWeek <= ubound)
            BackingValues.Add(existingWeek);
        else if (WeekOnly.TryParse(value.ToString() ?? "", out var parsedWeek) &&
                 parsedWeek.ToDateOnly() >= lbound && parsedWeek.ToDateOnly() <= ubound)
            BackingValues.Add(parsedWeek.ToDateOnly());
        else
            IsSatisfied = false;
    }
}