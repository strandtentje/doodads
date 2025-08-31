namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
public class ValidatingColorCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (value.ToString() is string stringValue && IsSatisfied)
        {
            IsSatisfied &= stringValue.Length is 4 or 7;
            IsSatisfied &= stringValue.ElementAt(0) == '#';
            IsSatisfied &= stringValue[1..].All(char.IsAsciiHexDigit);
            transformed = stringValue;
        }
        else
            IsSatisfied = false;
        if (IsSatisfied)
            BackingValues.Add(value);
    }
    public bool IsSatisfied { get; private set; } = true;
    public IEnumerable ValidItems => BackingValues;
}