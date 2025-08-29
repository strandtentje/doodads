namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class DateFieldValidatingCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= value is DateOnly dateOnly || DateOnly.TryParse(value.ToString(), out dateOnly);
        if (IsSatisfied) BackingValues.Add(dateOnly);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}