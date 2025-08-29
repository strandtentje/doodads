namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class TimeFieldBoundsValidatingCollection(TimeOnly lbound,TimeOnly ubound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= value is TimeOnly to || TimeOnly.TryParse(value.ToString(), out to);
        IsSatisfied &= to >= lbound && to <= ubound;
        if (IsSatisfied) BackingValues.Add(to);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}