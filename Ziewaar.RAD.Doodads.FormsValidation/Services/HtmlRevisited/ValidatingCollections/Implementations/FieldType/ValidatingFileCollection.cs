namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingFileCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        IsSatisfied &= value is Stream;
        if (IsSatisfied)
            BackingValues.Add(value);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}