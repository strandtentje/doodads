namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class OptionsValidatingCollection(string[] validOptions) : IValidatingCollection
{
    private readonly List<object> ValueStore = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        if (validOptions.Any())
        {
            if (value is Stream)
                IsSatisfied = false;
            else
            {
                transformed = value.ToString() ?? "";
                IsSatisfied &= validOptions.Contains(transformed);
                if (IsSatisfied)
                    ValueStore.Add(transformed);
            } 
        }
        else
        {
            ValueStore.Add(value);
        }
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => ValueStore;
}