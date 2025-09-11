namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
public class ValidatingColorCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed = value;
        if (!IsSatisfied) return;
        transformed = value;
        if (value.ToString() is { } stringValue)
        {
            IsSatisfied &= stringValue.Length is 4 or 7;
            if (!IsSatisfied) { Reason = "Wrong length"; return; }
            IsSatisfied &= stringValue.ElementAt(0) == '#';
            if (!IsSatisfied) { Reason = "Missing prefix"; return; }
            IsSatisfied &= stringValue[1..].All(char.IsAsciiHexDigit);
            if (!IsSatisfied) { Reason = "Not a hex"; return; }
            transformed = stringValue;
        }
        else
        {
            Reason = "Not a string";
            IsSatisfied = false;
        }
        if (IsSatisfied)
            BackingValues.Add(value);
    }
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
}