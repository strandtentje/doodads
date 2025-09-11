namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;

public class ValidatingNumberCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new List<object>();

    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        transformed = value;
        IsSatisfied &= decimal.TryParse(
            value.ToString(),
            CultureInfo.InvariantCulture,
            out decimal validDecimal);
        if (IsSatisfied)
            BackingValues.Add(transformed = validDecimal);
        else
            Reason = "Bad format";
    }

    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
}