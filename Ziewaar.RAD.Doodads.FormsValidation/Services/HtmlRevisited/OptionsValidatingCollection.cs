namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class OptionsValidatingCollection(string[] validOptions) : IValidatingCollection
{
    private readonly List<object> ValueStore = new();
    public void Add(object value)
    {
        IsSatisfied &= !validOptions.Any() || (value is not Stream && validOptions.Contains(value.ToString()));
        if (IsSatisfied)
            ValueStore.Add(value);
        else 
            ValueStore.Clear();
    }
    public bool IsSatisfied { get; private set; } = !validOptions.Any();
    public IEnumerable ValidItems => ValueStore;
}