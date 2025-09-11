namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations;
public class OptionsValidatingCollection(string[] validOptions) : IValidatingCollection
{
    private readonly List<object> ValueStore = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        if (validOptions.Any())
        {
            transformed = value.ToString() ?? "";
            IsSatisfied &= validOptions.Contains(transformed);
            if (IsSatisfied)
                ValueStore.Add(transformed);
            else
                Reason = "Not among options";
        }
        else
        {
            ValueStore.Add(value);
        }
    }
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => ValueStore;
}