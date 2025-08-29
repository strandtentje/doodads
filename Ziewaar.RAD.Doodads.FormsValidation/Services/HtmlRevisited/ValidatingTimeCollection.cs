namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingTimeCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        IsSatisfied &= value is TimeOnly parsedTime || TimeOnly.TryParse(value.ToString(), out parsedTime);
        if (IsSatisfied)
            BackingValues.Add(parsedTime);
    }
    public bool IsSatisfied { get; private set; }
    public IEnumerable ValidItems => BackingValues;
}