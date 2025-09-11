namespace Ziewaar.RAD.Doodads.FormsValidation.Services.EncTypeAgnostic.ValidatingCollections.Implementations.FieldType;
public class ValidatingEmailCollection : IValidatingCollection
{
    private readonly List<object> BackingValues = new();
    public void Add(object value, out object transformed)
    {
        transformed= value;
        if (!IsSatisfied) return;
        IsSatisfied &= EmailValidation.EmailValidator.Validate((transformed = value).ToString());
        if (IsSatisfied)
            BackingValues.Add(value);
        else
            Reason = "Bad e-mail";
    }
    public bool IsSatisfied { get; private set; } = true;
    public string Reason { get; private set; } = "";
    public IEnumerable ValidItems => BackingValues;
}