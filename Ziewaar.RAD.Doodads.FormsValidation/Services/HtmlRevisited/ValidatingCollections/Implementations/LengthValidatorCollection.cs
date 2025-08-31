namespace Ziewaar.RAD.Doodads.FormsValidation.Services.HtmlRevisited;
public class LengthValidatorCollection(uint minLength, uint maxLength) : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        var asString = value.ToString() ?? "";
        if (asString.Length < minLength)
            IsSatisfied = false;
        if (asString.Length > maxLength)
            IsSatisfied = false;
        if (IsSatisfied)
            BackingValues.Add(transformed = asString);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}