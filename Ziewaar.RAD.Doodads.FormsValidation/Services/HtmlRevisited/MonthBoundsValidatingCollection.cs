namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class MonthBoundsValidatingCollection(DateOnly lBound, DateOnly uBound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value)
    {
        if (!IsSatisfied)
            return;
        IsSatisfied &= value is DateOnly dy || DateOnly.TryParse($"{value}-01", out dy);
        IsSatisfied &= dy >= lBound &&  dy <= uBound;
        if (IsSatisfied)
            BackingValues.Add(dy);
    }
}