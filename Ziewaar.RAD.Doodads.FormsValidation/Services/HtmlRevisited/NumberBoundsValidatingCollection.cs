namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class NumberBoundsValidatingCollection(decimal lBound, decimal uBound) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value)
    {
        if (!IsSatisfied)
            return;
        IsSatisfied &= value is decimal dy || decimal.TryParse($"{value}", out dy);
        IsSatisfied &= dy >= lBound &&  dy <= uBound;
        if (IsSatisfied)
            BackingValues.Add(dy);
    }
}