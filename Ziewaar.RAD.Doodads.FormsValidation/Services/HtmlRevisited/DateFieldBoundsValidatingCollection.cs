namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class DateFieldBoundsValidatingCollection(DateOnly lbound, DateOnly ubound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= value is DateOnly dateOnly || DateOnly.TryParse(value.ToString(), out dateOnly);
        IsSatisfied &= dateOnly >= lbound && dateOnly <= ubound;
        if (IsSatisfied) BackingValues.Add(dateOnly);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}