namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class ValidatingNumberCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new List<object>();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        IsSatisfied &= decimal.TryParse(
            value.ToString(),
            CultureInfo.InvariantCulture, 
            out decimal validDecimal);
        if (IsSatisfied)
            BackingValues.Add(transformed = validDecimal);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}