namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingWeekCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        if (!IsSatisfied) return;
        if (value is DateOnly existingWeek)
            BackingValues.Add(existingWeek);
        else if (WeekOnly.TryParse(value.ToString() ?? "", out var parsedWeek))
            BackingValues.Add(parsedWeek.ToDateOnly());
        else
            IsSatisfied = false;
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}