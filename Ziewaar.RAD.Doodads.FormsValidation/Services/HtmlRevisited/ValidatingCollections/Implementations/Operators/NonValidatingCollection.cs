namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class NonValidatingCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        BackingValues.Add(value);
        transformed = value;
    }
    public bool IsSatisfied { get; } = true;
    public IEnumerable ValidItems =>  BackingValues;
}