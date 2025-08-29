namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class NonValidatingCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value)
    {
        BackingValues.Add(value);
    }
    public bool IsSatisfied { get; } = true;
    public IEnumerable ValidItems =>  BackingValues;
}