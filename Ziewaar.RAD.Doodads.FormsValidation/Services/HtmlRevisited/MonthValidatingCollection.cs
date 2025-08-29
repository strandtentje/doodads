namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class MonthValidatingCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= DateOnly.TryParse($"{value}-01", out var weekStart);
        if (IsSatisfied)
            BackingValues.Add(weekStart);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}