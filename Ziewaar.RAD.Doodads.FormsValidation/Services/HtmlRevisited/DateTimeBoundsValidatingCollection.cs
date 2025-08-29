namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class DateTimeBoundsValidatingCollection(DateTime lBound, DateTime uBound) : IValidatingCollection
{
    public List<object> BackingValues = new();
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
    public void Add(object value)
    {
        if (!IsSatisfied) return;
        IsSatisfied &= value is DateTime dt || DateTime.TryParse(value.ToString(), out dt);
        IsSatisfied &= dt >= lBound && dt <= uBound;
        if (IsSatisfied)
            BackingValues.Add(dt);
    }
}